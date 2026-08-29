using Xunit;
using Microsoft.AspNetCore.Identity;
using Moq;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Handlers.AuthHandler;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.UnitTests.Handlers
{
    public class DeleteAccountCommandHandlerTests
    {
        private static UserManager<AppUser> CreateUserManagerMock()
        {
            return new Mock<UserManager<AppUser>>(
                new Mock<IUserStore<AppUser>>().Object,
                null, null, null, null, null, null, null, null).Object;
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_Returns404()
        {
            var userManager = CreateUserManagerMock();
            Mock.Get(userManager)
                .Setup(u => u.FindByEmailAsync("missing@test.com"))
                .ReturnsAsync((AppUser)null!);

            var handler = new DeleteAccountCommandHandler(userManager);

            var result = await handler.Handle(
                new DeleteAccountCommand("missing@test.com", "Password123"), CancellationToken.None);

            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Handle_WhenPasswordIsWrong_Returns400_AndDoesNotDelete()
        {
            var userManager = CreateUserManagerMock();
            var user = new AppUser { Id = "user-1", Email = "user@test.com", IsDeleted = false };

            var mock = Mock.Get(userManager);
            mock.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
            mock.Setup(u => u.CheckPasswordAsync(user, "WrongPassword")).ReturnsAsync(false);

            var handler = new DeleteAccountCommandHandler(userManager);

            var result = await handler.Handle(
                new DeleteAccountCommand("user@test.com", "WrongPassword"), CancellationToken.None);

            Assert.Equal(400, result.StatusCode);
            Assert.False(user.IsDeleted);
            mock.Verify(u => u.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPasswordIsMissing_Returns400()
        {
            var userManager = CreateUserManagerMock();
            var user = new AppUser { Id = "user-1", Email = "user@test.com", IsDeleted = false };

            var mock = Mock.Get(userManager);
            mock.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);

            var handler = new DeleteAccountCommandHandler(userManager);

            var result = await handler.Handle(
                new DeleteAccountCommand("user@test.com", ""), CancellationToken.None);

            Assert.Equal(400, result.StatusCode);
            Assert.False(user.IsDeleted);
        }

        [Fact]
        public async Task Handle_WhenPasswordIsCorrect_SoftDeletesAndReturns200()
        {
            var userManager = CreateUserManagerMock();
            var user = new AppUser { Id = "user-1", Email = "user@test.com", IsDeleted = false };

            var mock = Mock.Get(userManager);
            mock.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
            mock.Setup(u => u.CheckPasswordAsync(user, "CorrectPassword")).ReturnsAsync(true);
            mock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var handler = new DeleteAccountCommandHandler(userManager);

            var result = await handler.Handle(
                new DeleteAccountCommand("user@test.com", "CorrectPassword"), CancellationToken.None);

            Assert.Equal(200, result.StatusCode);
            Assert.True(user.IsDeleted);
            mock.Verify(u => u.UpdateAsync(user), Times.Once);
        }
    }
}
