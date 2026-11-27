using ClinicSpace.Api.Contracts;
using ClinicSpace.Application.Health;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSpace.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(IHealthService healthService) : ControllerBase
{
    /// <summary>Reports whether the API is running and can reach its database.</summary>
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var report = await healthService.CheckAsync(cancellationToken);

        if (!report.DatabaseConnected)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Database unavailable",
                detail: "The API is running but cannot connect to its database.");
        }

        return Ok(new HealthResponse(Status: "OK", Database: "connected"));
    }
}
