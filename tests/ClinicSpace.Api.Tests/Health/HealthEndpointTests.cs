using System.Net;
using System.Net.Http.Json;
using ClinicSpace.Api.Contracts;
using ClinicSpace.Api.Tests.TestSupport;

namespace ClinicSpace.Api.Tests.Health;

public class HealthEndpointTests(ClinicSpaceApiFactory factory) : IClassFixture<ClinicSpaceApiFactory>
{
    [Fact]
    public async Task GetHealth_WhenDatabaseReachable_Returns200WithConnectedStatus()
    {
        var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;

        var response = await client.GetAsync("/api/health", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>(ct);
        Assert.NotNull(body);
        Assert.Equal("OK", body.Status);
        Assert.Equal("connected", body.Database);
    }
}
