using ClinicSpace.Domain.Auth;
using ClinicSpace.Infrastructure.Identity;
using ClinicSpace.Infrastructure.Tests.TestSupport;

namespace ClinicSpace.Infrastructure.Tests.Identity;

public class AuthServiceTests
{
    private static readonly DemoUser Admin = DevDataSeeder.DemoUsers.First(u => u.Role == Roles.Admin);
    private static readonly DemoUser Requester = DevDataSeeder.DemoUsers.First(u => u.Role == Roles.Requester);

    public static TheoryData<string, string, string, string> ValidUsers() => new()
    {
        { Admin.Email, Admin.Password, Admin.FullName, Roles.Admin },
        { Requester.Email, Requester.Password, Requester.FullName, Roles.Requester },
    };

    [Theory]
    [MemberData(nameof(ValidUsers))]
    public async Task LoginAsync_ValidCredentials_ReturnsUserWithRoleAndSignsIn(
        string email, string password, string fullName, string role)
    {
        await using var host = await CreateSeededHostAsync();

        var result = await host.Get<IAuthService>().LoginAsync(email, password, TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.Null(result.Error);
        Assert.NotNull(result.User);
        Assert.False(string.IsNullOrEmpty(result.User.Id));
        Assert.Equal(fullName, result.User.FullName);
        Assert.Equal(email, result.User.Email);
        Assert.Equal(role, result.User.Role);
        Assert.NotNull(host.Authentication.SignedInPrincipal);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_FailsWithGenericMessage()
    {
        await using var host = await CreateSeededHostAsync();

        var result = await host.Get<IAuthService>()
            .LoginAsync(Requester.Email, "Wrong-Password1!", TestContext.Current.CancellationToken);

        AssertGenericFailure(result);
        Assert.Null(host.Authentication.SignedInPrincipal);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_FailsWithSameGenericMessage()
    {
        await using var host = await CreateSeededHostAsync();

        var result = await host.Get<IAuthService>()
            .LoginAsync("nobody@example.nhs.uk", Requester.Password, TestContext.Current.CancellationToken);

        AssertGenericFailure(result);
        Assert.Null(host.Authentication.SignedInPrincipal);
    }

    [Fact]
    public async Task LogoutAsync_SignsOut()
    {
        await using var host = await CreateSeededHostAsync();

        await host.Get<IAuthService>().LogoutAsync();

        Assert.True(host.Authentication.SignedOut);
    }

    private static async Task<IdentityTestHost> CreateSeededHostAsync()
    {
        var host = new IdentityTestHost();
        await host.Get<DevDataSeeder>().SeedAsync(TestContext.Current.CancellationToken);
        return host;
    }

    private static void AssertGenericFailure(AuthResult result)
    {
        Assert.False(result.Succeeded);
        Assert.Null(result.User);
        Assert.Equal(AuthResult.InvalidCredentialsMessage, result.Error);
    }
}
