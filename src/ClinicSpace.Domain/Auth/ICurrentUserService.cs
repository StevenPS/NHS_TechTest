using System.Security.Claims;

namespace ClinicSpace.Domain.Auth;

/// <summary>Looks up the profile and role of an authenticated principal.</summary>
public interface ICurrentUserService
{
    /// <summary>Returns the user behind <paramref name="principal"/>, or null if there is none.</summary>
    Task<CurrentUser?> GetAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
