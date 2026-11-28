using System.Security.Claims;
using ClinicSpace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClinicSpace.Infrastructure.Tests.TestSupport;

/// <summary>
/// The real Identity setup (<see cref="DependencyInjection.AddClinicSpaceIdentity"/>) over an in-memory SQLite
/// database, with a fake HTTP request so <c>SignInManager</c> can run. Cookie writes are recorded by
/// <see cref="RecordingAuthenticationService"/> instead of going to a response.
/// </summary>
public sealed class IdentityTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _root;
    private readonly AsyncServiceScope _scope;

    public IdentityTestHost()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        services.AddClinicSpaceIdentity();

        // A fixed accessor: the default one stores the context in an AsyncLocal, which does not flow
        // back out of async helpers that create the host.
        var accessor = new FixedHttpContextAccessor();
        services.AddSingleton<IHttpContextAccessor>(accessor);
        services.RemoveAll<IAuthenticationService>();
        services.AddSingleton<IAuthenticationService>(Authentication);

        _root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        _scope = _root.CreateAsyncScope();

        HttpContext = new DefaultHttpContext { RequestServices = _scope.ServiceProvider };
        accessor.HttpContext = HttpContext;

        Get<AppDbContext>().Database.EnsureCreated();
    }

    public RecordingAuthenticationService Authentication { get; } = new();

    public HttpContext HttpContext { get; }

    /// <summary>Resolves a service from the single request scope.</summary>
    public T Get<T>() where T : notnull => _scope.ServiceProvider.GetRequiredService<T>();

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _root.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

internal sealed class FixedHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}

/// <summary>Records sign-in and sign-out calls instead of writing cookies.</summary>
public sealed class RecordingAuthenticationService : IAuthenticationService
{
    public ClaimsPrincipal? SignedInPrincipal { get; private set; }

    public bool SignedOut { get; private set; }

    public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
    {
        SignedInPrincipal = principal;
        return Task.CompletedTask;
    }

    public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        SignedOut = true;
        return Task.CompletedTask;
    }

    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) =>
        Task.FromResult(AuthenticateResult.NoResult());

    public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

    public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
}
