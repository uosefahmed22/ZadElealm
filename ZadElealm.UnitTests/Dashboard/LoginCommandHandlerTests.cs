using AdminDashboard.Commands.AdminCommand;
using AdminDashboard.Handlers.AuthHandler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ZadElealm.Core.Models.Identity;
using Xunit;

namespace ZadElealm.UnitTests.Dashboard;

public sealed class LoginCommandHandlerTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Handle_AllowsOnlyUsersInAdminRole(bool isAdmin, bool expectedSuccess)
    {
        var user = new AppUser { Id = "user-1", Email = "admin@test.com", UserName = "admin@test.com" };
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManager.Setup(manager => manager.IsInRoleAsync(user, "Admin")).ReturnsAsync(isAdmin);

        var signInManager = CreateSignInManager(userManager.Object);
        signInManager
            .Setup(manager => manager.CheckPasswordSignInAsync(user, "valid-password", true))
            .ReturnsAsync(SignInResult.Success);

        var handler = new LoginCommandHandler(userManager.Object, signInManager.Object);

        var result = await handler.Handle(new LoginCommand
        {
            Email = user.Email,
            Password = "valid-password"
        }, CancellationToken.None);

        Assert.Equal(expectedSuccess, result.Succeeded);
        signInManager.Verify(
            manager => manager.CheckPasswordSignInAsync(user, "valid-password", true),
            Times.Once);
    }

    private static Mock<UserManager<AppUser>> CreateUserManager()
        => new(
            Mock.Of<IUserStore<AppUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<AppUser>>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<AppUser>>>());

    private static Mock<SignInManager<AppUser>> CreateSignInManager(UserManager<AppUser> userManager)
        => new(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<AppUser>>>(),
            Mock.Of<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<AppUser>>());
}
