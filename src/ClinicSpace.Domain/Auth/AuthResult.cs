namespace ClinicSpace.Domain.Auth;

/// <summary>Outcome of a sign-in attempt.</summary>
public sealed record AuthResult(bool Succeeded, CurrentUser? User, string? Error)
{
    /// <summary>
    /// The only failure message. It is the same for an unknown email and a wrong password,
    /// so the response never reveals which accounts exist.
    /// </summary>
    public const string InvalidCredentialsMessage = "Invalid email or password.";

    public static AuthResult Success(CurrentUser user) => new(true, user, null);

    public static AuthResult Failure() => new(false, null, InvalidCredentialsMessage);
}
