using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Service.AppServices;

namespace ZadElealm.UnitTests.Services;

public class AppServiceValidationTests
{
    [Fact]
    public async Task SendEmail_WhenRecipientIsInvalid_Returns400BeforeConnecting()
    {
        var settings = Options.Create(new EmailSettings
        {
            Email = "sender@example.com",
            Password = "not-used",
            SmtpServer = "smtp.example.com",
            Port = 587
        });
        var message = new EmailMessage
        {
            To = "not-an-email",
            Subject = "Subject",
            Body = "Body"
        };

        var result = await new SendEmailService(
            settings,
            NullLogger<SendEmailService>.Instance).SendEmailAsync(message);

        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task SendEmail_WhenSmtpCredentialsAreMissing_ReturnsActionable503()
    {
        var settings = Options.Create(new EmailSettings
        {
            Email = string.Empty,
            Password = string.Empty,
            SmtpServer = "smtp.gmail.com",
            Port = 587
        });
        var message = new EmailMessage
        {
            To = "recipient@example.com",
            Subject = "رمز التحقق",
            Body = "Body"
        };

        var result = await new SendEmailService(
            settings,
            NullLogger<SendEmailService>.Instance).SendEmailAsync(message);

        Assert.Equal(503, result.StatusCode);
        Assert.Contains("غير مهيأة", result.Message);
    }

    [Fact]
    public async Task UploadImage_WhenExtensionAndContentTypeDisagree_Returns400()
    {
        var cloudinary = new Cloudinary(new Account("cloud", "key", "secret"));
        await using var stream = new MemoryStream([1, 2, 3]);
        IFormFile file = new FormFile(stream, 0, stream.Length, "image", "image.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var result = await new ImageService(cloudinary).UploadImageAsync(file);

        Assert.Equal(400, result.StatusCode);
    }
}
