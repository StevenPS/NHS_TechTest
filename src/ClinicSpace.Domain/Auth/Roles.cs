namespace ClinicSpace.Domain.Auth;

/// <summary>Role names, stored in AspNetRoles. Names match the exercise's user data.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Requester = "Requester";

    public static IReadOnlyList<string> All { get; } = [Admin, Requester];
}
