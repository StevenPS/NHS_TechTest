using ClinicSpace.Application.Abstractions;

namespace ClinicSpace.Application.Health;

public sealed class HealthService(IAppDbContext db) : IHealthService
{
    private readonly IAppDbContext _db = db;

    public Task<HealthReport> CheckAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
