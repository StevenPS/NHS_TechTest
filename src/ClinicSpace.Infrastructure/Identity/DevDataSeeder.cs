using ClinicSpace.Domain.Auth;
using Microsoft.AspNetCore.Identity;

namespace ClinicSpace.Infrastructure.Identity;

/// <summary>A demo account, copied from the Users sheet of the exercise data spreadsheet.</summary>
public sealed record DemoUser(string FullName, string Email, string Role, string Password);

/// <summary>
/// Creates the roles and the demo users for local development. Safe to run on every start:
/// it only adds what is missing and never changes existing accounts.
/// </summary>
public sealed class DevDataSeeder(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
{
    private const string AdminPassword = "AdminPass!23";
    private const string RequesterPassword = "Password123!";

    /// <summary>
    /// The users from <c>docs/Practical Exercise Data - Applications Developer B6.xlsx</c> (Users sheet).
    /// Development only: these passwords are published with the exercise.
    /// </summary>
    public static IReadOnlyList<DemoUser> DemoUsers { get; } =
    [
        new("Pat Jones", "pat.jones@example.nhs.uk", Roles.Admin, AdminPassword),
        new("Aman Singh", "aman.singh@example.nhs.uk", Roles.Admin, AdminPassword),
        new("Sam Taylor", "sam.taylor@example.nhs.uk", Roles.Admin, AdminPassword),
        new("Jordan Brown", "jordan.brown@example.nhs.uk", Roles.Admin, AdminPassword),
        new("Riley Green", "riley.green@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Alex Evans", "alex.evans@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Morgan Wood", "morgan.wood@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Sara Khan", "sara.khan@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Jamie Clarke", "jamie.clarke@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Lee Wilson", "lee.wilson@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Casey Hughes", "casey.hughes@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Taylor Patel", "taylor.patel@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Drew Cooper", "drew.cooper@example.nhs.uk", Roles.Requester, RequesterPassword),
        new("Kai Morris", "kai.morris@example.nhs.uk", Roles.Requester, RequesterPassword),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var role in Roles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole(role)), $"create role '{role}'");
            }
        }

        foreach (var demo in DemoUsers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await userManager.FindByEmailAsync(demo.Email) is not null)
            {
                continue;
            }

            var user = new AppUser
            {
                UserName = demo.Email,
                Email = demo.Email,
                EmailConfirmed = true,
                FullName = demo.FullName,
            };
            EnsureSucceeded(await userManager.CreateAsync(user, demo.Password), $"create user '{demo.Email}'");
            EnsureSucceeded(await userManager.AddToRoleAsync(user, demo.Role), $"add '{demo.Email}' to role '{demo.Role}'");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Seeding failed to {action}: {errors}");
        }
    }
}
