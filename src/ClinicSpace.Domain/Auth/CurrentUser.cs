namespace ClinicSpace.Domain.Auth;

/// <summary>The signed-in user, as the rest of the application sees them.</summary>
public sealed record CurrentUser(string Id, string FullName, string Email, string Role);
