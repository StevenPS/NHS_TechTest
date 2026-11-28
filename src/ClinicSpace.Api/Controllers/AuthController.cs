using ClinicSpace.Api.Contracts;
using ClinicSpace.Domain.Auth;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSpace.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
    ICurrentUserService currentUserService,
    IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>Readable by the SPA, which echoes it back in <see cref="AntiforgeryHeaderName"/>.</summary>
    public const string AntiforgeryCookieName = "XSRF-TOKEN";
    public const string AntiforgeryHeaderName = "X-XSRF-TOKEN";

    /// <summary>
    /// Issues an antiforgery token. The token is bound to the current user, so the SPA calls this on load
    /// and again after signing in or out.
    /// </summary>
    [HttpGet("antiforgery")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append(AntiforgeryCookieName, tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
        });
        return NoContent();
    }

    /// <summary>Signs in with email and password and sets the authentication cookie.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);

        if (!result.Succeeded || result.User is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Sign-in failed", detail: result.Error);
        }

        return Ok(ToResponse(result.User));
    }

    /// <summary>Signs out and clears the authentication cookie.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();
        return NoContent();
    }

    /// <summary>The signed-in user's name, email and role.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetAsync(User, cancellationToken);

        // A valid cookie for a user that no longer exists.
        return user is null ? Problem(statusCode: StatusCodes.Status401Unauthorized) : Ok(ToResponse(user));
    }

    private static CurrentUserResponse ToResponse(CurrentUser user) => new(user.FullName, user.Email, user.Role);
}
