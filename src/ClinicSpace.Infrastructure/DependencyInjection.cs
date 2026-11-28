using ClinicSpace.Application.Abstractions;
using ClinicSpace.Domain.Auth;
using ClinicSpace.Infrastructure.Identity;
using ClinicSpace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicSpace.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DefaultConnection";
    public const string AuthCookieName = ".ClinicSpace.Auth";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // The connection string is resolved lazily so test hosts can swap the provider
        // without needing a SQL Server connection string.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddClinicSpaceIdentity();

        services.ConfigureApplicationCookie(options =>
        {
            // Identity's defaults: 14 days, sliding.
            options.Cookie.Name = AuthCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;

            // An API answers with status codes; the SPA decides where to send the user.
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        return services;
    }

    /// <summary>
    /// Identity (users, roles, sign-in) and the auth services. Separate from <see cref="AddInfrastructure"/>
    /// so tests can build the same Identity setup over their own database.
    /// </summary>
    public static IServiceCollection AddClinicSpaceIdentity(this IServiceCollection services)
    {
        // Identity's default password policy; no self-registration, so users only come from the seeder.
        services.AddIdentity<AppUser, IdentityRole>(options => options.User.RequireUniqueEmail = true)
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<DevDataSeeder>();

        return services;
    }
}
