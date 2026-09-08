using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Handlers.AuthHandler;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.UnitTests.Handlers;

public sealed class UpdateEmailCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailIsUpdated_SendsOneMessageToTheNewAddress()
    {
        var user = new AppUser
        {
            Id = "user-1",
            Email = "old@example.test",
            UserName = "old",
            DisplayName = "طالب"
        };
        var userManager = CreateUserManager();
        Mock.Get(userManager)
            .Setup(manager => manager.FindByIdAsync(user.Id))
            .ReturnsAsync(user);
        Mock.Get(userManager)
            .Setup(manager => manager.SetEmailAsync(user, "new@example.test"))
            .ReturnsAsync(IdentityResult.Success);
        Mock.Get(userManager)
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var otpService = new Mock<IOtpService>();
        otpService
            .Setup(service => service.IsValidOtp("new@example.test", "123456"))
            .Returns(true);
        var emailService = new Mock<ISendEmailService>();
        emailService
            .Setup(service => service.SendEmailAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiDataResponse(200));
        var handler = new UpdateEmailCommandHandler(
            userManager,
            otpService.Object,
            emailService.Object);

        var result = await handler.Handle(
            new UpdateEmailCommand(user.Id, "new@example.test", "123456"),
            CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        emailService.Verify(
            service => service.SendEmailAsync(
                It.Is<EmailMessage>(message =>
                    message.To == "new@example.test" &&
                    message.Subject == "تم تحديث البريد الإلكتروني"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        emailService.VerifyNoOtherCalls();
    }

    private static UserManager<AppUser> CreateUserManager()
    {
        return new Mock<UserManager<AppUser>>(
            new Mock<IUserStore<AppUser>>().Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null).Object;
    }
}
