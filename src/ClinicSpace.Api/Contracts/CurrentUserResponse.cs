namespace ClinicSpace.Api.Contracts;

/// <summary>The signed-in user, as the SPA sees them.</summary>
public sealed record CurrentUserResponse(string FullName, string Email, string Role);
