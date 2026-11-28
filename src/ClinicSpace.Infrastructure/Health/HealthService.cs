using ClinicSpace.Domain.Health;
using ClinicSpace.Infrastructure.Persistence;

namespace ClinicSpace.Infrastructure.Health;

public sealed class HealthService(AppDbContext db) : IHealthService
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
