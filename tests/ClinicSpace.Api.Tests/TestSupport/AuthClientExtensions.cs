using System.Net.Http.Json;
using ClinicSpace.Api.Contracts;
using ClinicSpace.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClinicSpace.Api.Tests.TestSupport;

public static class AuthClientExtensions
{
    /// <summary>
    /// A client that keeps cookies across requests. HTTPS, because the auth cookie is Secure
    /// and would not be sent back over plain HTTP.
    /// </summary>
    public static HttpClient CreateHttpsClient(this WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true,
        });

    /// <summary>Fetches an antiforgery token, as the SPA does, and sends it on every later request.</summary>
    public static async Task AddAntiforgeryTokenAsync(this HttpClient client, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync("/api/auth/antiforgery", cancellationToken);
        response.EnsureSuccessStatusCode();

        var prefix = AuthController.AntiforgeryCookieName + "=";
        var token = response.Headers.GetValues("Set-Cookie")
            .Single(c => c.StartsWith(prefix, StringComparison.Ordinal))[prefix.Length..]
            .Split(';')[0];

        client.DefaultRequestHeaders.Remove(AuthController.AntiforgeryHeaderName);
        client.DefaultRequestHeaders.Add(AuthController.AntiforgeryHeaderName, Uri.UnescapeDataString(token));
    }

    /// <summary>Signs in, then refreshes the antiforgery token, which is bound to the signed-in user.</summary>
    public static async Task SignInAsync(this HttpClient client, string email, string password, CancellationToken cancellationToken)
    {
        await client.AddAntiforgeryTokenAsync(cancellationToken);
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password), cancellationToken);
        response.EnsureSuccessStatusCode();
        await client.AddAntiforgeryTokenAsync(cancellationToken);
    }
}
