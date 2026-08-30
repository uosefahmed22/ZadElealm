using AdminDashboard.Commands.UserCommand;
using AdminDashboard.Handlers.UserHandler;
using AdminDashboard.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using ZadElealm.Core.Models.Identity;
using Xunit;

namespace ZadElealm.UnitTests.Dashboard;

public sealed class UpdateUserRolesCommandHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_PersistsEmailConfirmationState(bool isConfirmed)
    {
        var user = new AppUser
        {
            Id = "user-1",
            DisplayName = "Old name",
            EmailConfirmed = !isConfirmed
        };
        var userManager = CreateUserManagerMock();
        userManager.Setup(manager => manager.FindByIdAsync(user.Id)).ReturnsAsync(user);
        userManager.Setup(manager => manager.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.GetRolesAsync(user)).ReturnsAsync([]);

        var handler = new UpdateUserRolesCommandHandler(userManager.Object);
        var command = new UpdateUserRolesCommand
        {
            Model = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = "New name",
                IsConfirmed = isConfirmed,
                Roles = []
            }
        };

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(isConfirmed, user.EmailConfirmed);
        userManager.Verify(manager => manager.UpdateAsync(
            It.Is<AppUser>(updated => updated.EmailConfirmed == isConfirmed)), Times.Once);
    }

    private static Mock<UserManager<AppUser>> CreateUserManagerMock()
        => new(
            new Mock<IUserStore<AppUser>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
}
