namespace ClinicSpace.Domain.Auth;

/// <summary>Signs users in and out. Signing in issues the authentication cookie.</summary>
public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync();
}
