namespace ClinicSpace.Domain.Health;

public interface IHealthService
{
    Task<HealthReport> CheckAsync(CancellationToken cancellationToken = default);
}
