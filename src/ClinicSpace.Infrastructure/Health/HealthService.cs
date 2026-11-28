using ClinicSpace.Application.Abstractions;

namespace ClinicSpace.Application.Health;

public sealed class HealthService(IAppDbContext db) : IHealthService
{
    public async Task<HealthReport> CheckAsync(CancellationToken cancellationToken = default)
    {
        bool connected;
        try
        {
            connected = await db.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // A health probe reports failure; it never throws for an unreachable database.
            connected = false;
        }

        return new HealthReport(connected);
    }
}
