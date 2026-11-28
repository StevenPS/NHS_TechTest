using ClinicSpace.Domain.Auth;
using ClinicSpace.Infrastructure.Identity;
using ClinicSpace.Infrastructure.Tests.TestSupport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicSpace.Infrastructure.Tests.Identity;

public class DevDataSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesBothRoles()
    {
        await using var host = new IdentityTestHost();

        await host.Get<DevDataSeeder>().SeedAsync(TestContext.Current.CancellationToken);

        var roles = host.Get<RoleManager<IdentityRole>>();
        Assert.True(await roles.RoleExistsAsync(Roles.Admin));
        Assert.True(await roles.RoleExistsAsync(Roles.Requester));
    }

    [Fact]
    public async Task SeedAsync_CreatesEveryDemoUserWithNameRoleAndPassword()
    {
        await using var host = new IdentityTestHost();

        await host.Get<DevDataSeeder>().SeedAsync(TestContext.Current.CancellationToken);

        var users = host.Get<UserManager<AppUser>>();
        Assert.Equal(14, DevDataSeeder.DemoUsers.Count);
        foreach (var demo in DevDataSeeder.DemoUsers)
        {
            var user = await users.FindByEmailAsync(demo.Email);
            Assert.NotNull(user);
            Assert.Equal(demo.Email, user.UserName);
            Assert.Equal(demo.FullName, user.FullName);
            Assert.Equal([demo.Role], await users.GetRolesAsync(user));
            Assert.True(await users.CheckPasswordAsync(user, demo.Password));
        }
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateUsersOrRoles()
    {
        await using var host = new IdentityTestHost();
        var seeder = host.Get<DevDataSeeder>();
        var ct = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(ct);
        await seeder.SeedAsync(ct);

        Assert.Equal(DevDataSeeder.DemoUsers.Count, await host.Get<UserManager<AppUser>>().Users.CountAsync(ct));
        Assert.Equal(Roles.All.Count, await host.Get<RoleManager<IdentityRole>>().Roles.CountAsync(ct));
    }
}
