using ClinicSpace.Application.Health;
using ClinicSpace.Application.Tests.TestSupport;
using ClinicSpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicSpace.Application.Tests.Health;

public class HealthServiceTests
{
    [Fact]
    public async Task CheckAsync_WhenDatabaseReachable_ReportsConnected()
    {
        using var database = new SqliteTestDatabase();
        await using var context = database.CreateContext();
        var service = new HealthService(context);

        var report = await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.True(report.DatabaseConnected);
    }

    [Fact]
    public async Task CheckAsync_WhenDatabaseUnreachable_ReportsDisconnected()
    {
        var missingFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={missingFile};Mode=ReadOnly")
            .Options;
        await using var context = new AppDbContext(options);
        var service = new HealthService(context);

        var report = await service.CheckAsync(TestContext.Current.CancellationToken);

        Assert.False(report.DatabaseConnected);
    }
}
