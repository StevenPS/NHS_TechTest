using Microsoft.AspNetCore.Identity;

namespace ClinicSpace.Infrastructure.Identity;

/// <summary>Application user. Signs in by email; <see cref="IdentityUser.UserName"/> is set to the email.</summary>
public class AppUser : IdentityUser
{
    public const int FullNameMaxLength = 200;

    public string FullName { get; set; } = string.Empty;
}
