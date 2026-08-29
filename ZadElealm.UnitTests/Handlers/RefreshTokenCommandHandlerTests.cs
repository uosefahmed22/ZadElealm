using Xunit;
using Microsoft.AspNetCore.Identity;
using Moq;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Handlers.Auth;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.UnitTests.Handlers
{
    public class RefreshTokenCommandHandlerTests
    {
        private static RefreshTokenCommand BuildCommand()
            => new RefreshTokenCommand(new TokenRequestDto { Token = "access-token", RefreshToken = "refresh-token" });

        [Fact]
        public async Task Handle_WhenRefreshFails_Returns401()
        {
            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(t => t.RefreshToken("access-token", "refresh-token"))
                .ReturnsAsync(new AuthResult { Result = false, message = "رمز تحديث غير صالح." });

            var handler = new RefreshTokenCommandHandler(tokenService.Object);

            var result = await handler.Handle(BuildCommand(), CancellationToken.None);

            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task Handle_WhenRefreshSucceeds_Returns200WithNewTokenData()
        {
            var userData = new UserDTO
            {
                DisplayName = "Test",
                Email = "test@test.com",
                Token = "new-access-token",
                RefreshToken = "new-refresh-token"
            };

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(t => t.RefreshToken("access-token", "refresh-token"))
                .ReturnsAsync(new AuthResult { Result = true, message = "تم تحديث الرمز.", UserData = userData });

            var handler = new RefreshTokenCommandHandler(tokenService.Object);

            var result = await handler.Handle(BuildCommand(), CancellationToken.None);

            Assert.Equal(200, result.StatusCode);
            var dataResponse = Assert.IsType<ApiDataResponse>(result);
            var returnedUser = Assert.IsType<UserDTO>(dataResponse.Data);
            Assert.Equal("new-access-token", returnedUser.Token);
            Assert.Equal("new-refresh-token", returnedUser.RefreshToken);
        }

        [Fact]
        public async Task Handle_WhenSuccessButNoUserData_Returns401()
        {
            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(t => t.RefreshToken("access-token", "refresh-token"))
                .ReturnsAsync(new AuthResult { Result = true, message = "تم تحديث الرمز.", UserData = null });

            var handler = new RefreshTokenCommandHandler(tokenService.Object);

            var result = await handler.Handle(BuildCommand(), CancellationToken.None);

            Assert.Equal(401, result.StatusCode);
        }
    }
}
