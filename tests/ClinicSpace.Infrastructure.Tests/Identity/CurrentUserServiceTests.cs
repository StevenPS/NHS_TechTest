using System.Security.Claims;
using ClinicSpace.Domain.Auth;
using ClinicSpace.Infrastructure.Identity;
using ClinicSpace.Infrastructure.Tests.TestSupport;
using Microsoft.AspNetCore.Identity;

namespace ClinicSpace.Infrastructure.Tests.Identity;

public class CurrentUserServiceTests
{
    [Fact]
    public async Task GetAsync_SignedInUser_ReturnsNameEmailAndRole()
    {
        await using var host = new IdentityTestHost();
        var ct = TestContext.Current.CancellationToken;
        await host.Get<DevDataSeeder>().SeedAsync(ct);
        var demo = DevDataSeeder.DemoUsers.First(u => u.Role == Roles.Admin);
        var user = await host.Get<UserManager<AppUser>>().FindByEmailAsync(demo.Email);
        Assert.NotNull(user);
        var principal = await host.Get<SignInManager<AppUser>>().CreateUserPrincipalAsync(user);

        var current = await host.Get<ICurrentUserService>().GetAsync(principal, ct);

        Assert.Equal(new CurrentUser(user.Id, demo.FullName, demo.Email, Roles.Admin), current);
    }

    [Fact]
    public async Task GetAsync_PrincipalForUnknownUser_ReturnsNull()
    {
        await using var host = new IdentityTestHost();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "Test"));

        var current = await host.Get<ICurrentUserService>().GetAsync(principal, TestContext.Current.CancellationToken);

        Assert.Null(current);
    }

    [Fact]
    public async Task GetAsync_AnonymousPrincipal_ReturnsNull()
    {
        await using var host = new IdentityTestHost();

        var current = await host.Get<ICurrentUserService>()
            .GetAsync(new ClaimsPrincipal(new ClaimsIdentity()), TestContext.Current.CancellationToken);

        Assert.Null(current);
    }
}
