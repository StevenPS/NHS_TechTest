using System.Net;
using System.Net.Http.Json;
using ClinicSpace.Api.Contracts;
using ClinicSpace.Api.Tests.TestSupport;
using ClinicSpace.Domain.Auth;
using ClinicSpace.Infrastructure;
using ClinicSpace.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSpace.Api.Tests.Auth;

public class AuthEndpointTests(ClinicSpaceApiFactory factory) : IClassFixture<ClinicSpaceApiFactory>
{
    private static readonly DemoUser Admin = DevDataSeeder.DemoUsers.First(u => u.Role == Roles.Admin);
    private static readonly DemoUser Requester = DevDataSeeder.DemoUsers.First(u => u.Role == Roles.Requester);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Login_ValidCredentials_SetsAuthCookieAndReturnsUser()
    {
        var client = factory.CreateHttpsClient();
        await client.AddAntiforgeryTokenAsync(Ct);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Admin.Email, Admin.Password), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith(DependencyInjection.AuthCookieName + "=", StringComparison.Ordinal)
                && c.Contains("httponly", StringComparison.OrdinalIgnoreCase));
        var user = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(Ct);
        Assert.Equal(new CurrentUserResponse(Admin.FullName, Admin.Email, Roles.Admin), user);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401ProblemWithGenericMessage()
    {
        var client = factory.CreateHttpsClient();
        await client.AddAntiforgeryTokenAsync(Ct);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Requester.Email, "Wrong-Password1!"), Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
        Assert.Equal(AuthResult.InvalidCredentialsMessage, problem?.Detail);
    }

    [Fact]
    public async Task Login_WithoutAntiforgeryToken_Returns400Problem()
    {
        var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Admin.Email, Admin.Password), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem?.Status);
    }

    [Fact]
    public async Task Login_MissingFields_Returns400ValidationProblem()
    {
        var client = factory.CreateHttpsClient();
        await client.AddAntiforgeryTokenAsync(Ct);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("", ""), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        Assert.NotNull(problem);
        Assert.Contains("Email", problem.Errors.Keys);
        Assert.Contains("Password", problem.Errors.Keys);
    }

    [Fact]
    public async Task Me_Anonymous_Returns401()
    {
        var client = factory.CreateHttpsClient();

        var response = await client.GetAsync("/api/auth/me", Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_SignedIn_Returns200WithNameEmailAndRole()
    {
        var client = factory.CreateHttpsClient();
        await client.SignInAsync(Requester.Email, Requester.Password, Ct);

        var response = await client.GetAsync("/api/auth/me", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(Ct);
        Assert.Equal(new CurrentUserResponse(Requester.FullName, Requester.Email, Roles.Requester), user);
    }

    [Fact]
    public async Task Logout_ThenMe_Returns401()
    {
        var client = factory.CreateHttpsClient();
        await client.SignInAsync(Admin.Email, Admin.Password, Ct);

        var logout = await client.PostAsync("/api/auth/logout", content: null, Ct);
        var me = await client.GetAsync("/api/auth/me", Ct);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }
}
