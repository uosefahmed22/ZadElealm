using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Handlers.Auth;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.UnitTests.Handlers;

public sealed class ResendConfirmationEmailCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSendingFails_Returns503AndDoesNotConsumeTheRetryWindow()
    {
        var email = $"send-failure-{Guid.NewGuid():N}@test.com";
        var user = new AppUser
        {
            Id = "user-1",
            Email = email,
            DisplayName = "طالب",
            EmailConfirmed = false
        };
        var userManager = CreateUserManager();
        Mock.Get(userManager).Setup(manager => manager.FindByEmailAsync(email)).ReturnsAsync(user);
        Mock.Get(userManager)
            .Setup(manager => manager.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");
        var emailService = new Mock<ISendEmailService>();
        emailService
            .Setup(service => service.SendEmailAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiDataResponse(500, null, "provider failed"));
        var handler = CreateHandler(userManager, emailService.Object);

        var first = await handler.Handle(
            new ResendConfirmationEmailCommand(email),
            CancellationToken.None);
        var second = await handler.Handle(
            new ResendConfirmationEmailCommand(email),
            CancellationToken.None);

        Assert.Equal(503, first.StatusCode);
        Assert.Equal(503, second.StatusCode);
        emailService.Verify(service => service.SendEmailAsync(
            It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenSendingSucceeds_EnforcesTheRetryWindow()
    {
        var email = $"rate-limit-{Guid.NewGuid():N}@test.com";
        var user = new AppUser
        {
            Id = "user-2",
            Email = email,
            DisplayName = "طالب",
            EmailConfirmed = false
        };
        var userManager = CreateUserManager();
        Mock.Get(userManager).Setup(manager => manager.FindByEmailAsync(email)).ReturnsAsync(user);
        Mock.Get(userManager)
            .Setup(manager => manager.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");
        var emailService = new Mock<ISendEmailService>();
        emailService
            .Setup(service => service.SendEmailAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiDataResponse(200));
        var handler = CreateHandler(userManager, emailService.Object);

        var first = await handler.Handle(
            new ResendConfirmationEmailCommand(email),
            CancellationToken.None);
        var second = await handler.Handle(
            new ResendConfirmationEmailCommand(email),
            CancellationToken.None);

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(429, second.StatusCode);
        emailService.Verify(service => service.SendEmailAsync(
            It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ResendConfirmationEmailCommandHandler CreateHandler(
        UserManager<AppUser> userManager,
        ISendEmailService emailService)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BaseUrl"] = "https://example.test"
            })
            .Build();
        return new ResendConfirmationEmailCommandHandler(
            userManager,
            emailService,
            configuration);
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
