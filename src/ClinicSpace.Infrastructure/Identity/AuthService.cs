using ClinicSpace.Domain.Auth;
using Microsoft.AspNetCore.Identity;

namespace ClinicSpace.Infrastructure.Identity;

public sealed class AuthService(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager) : IAuthService
{
    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return AuthResult.Failure();
        }

        // Lockout is off for this exercise; Entra would own account protection in a live deployment.
        var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return AuthResult.Failure();
        }

        return AuthResult.Success(await user.ToCurrentUserAsync(userManager));
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();
}
