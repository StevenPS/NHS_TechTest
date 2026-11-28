using System.Security.Claims;
using ClinicSpace.Domain.Auth;
using Microsoft.AspNetCore.Identity;

namespace ClinicSpace.Infrastructure.Identity;

public sealed class CurrentUserService(UserManager<AppUser> userManager) : ICurrentUserService
{
    public async Task<CurrentUser?> GetAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var user = await userManager.GetUserAsync(principal);
        return user is null ? null : await user.ToCurrentUserAsync(userManager);
    }
}

internal static class AppUserMapping
{
    /// <summary>Each user has exactly one role (see <see cref="DevDataSeeder"/>).</summary>
    public static async Task<CurrentUser> ToCurrentUserAsync(this AppUser user, UserManager<AppUser> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new CurrentUser(user.Id, user.FullName, user.Email ?? string.Empty, roles.FirstOrDefault() ?? string.Empty);
    }
}
