using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;
using ZadElealm.Core.Service;

namespace ZadElealm.IntegrationTests;

public sealed class CertificateFileAccessTests : IClassFixture<ZadElealmApiFactory>
{
    private static readonly byte[] PdfContent = "%PDF-1.4 secure certificate"u8.ToArray();
    private readonly ZadElealmApiFactory _factory;

    public CertificateFileAccessTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CertificateFile_IsAvailableOnlyToItsOwner()
    {
        var seeded = await SeedCertificateAsync();

        try
        {
            using var ownerClient = CreateClient(_factory.GenerateToken("User"));
            var ownerResponse = await ownerClient.GetAsync(
                $"/api/Certificate/{seeded.CertificateId}/file");

            Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
            Assert.Equal("application/pdf", ownerResponse.Content.Headers.ContentType?.MediaType);
            Assert.True(ownerResponse.Headers.CacheControl?.Private);
            Assert.True(ownerResponse.Headers.CacheControl?.NoStore);
            Assert.Equal(PdfContent, await ownerResponse.Content.ReadAsByteArrayAsync());

            using var otherClient = CreateClient(
                _factory.GenerateTokenForEmail(seeded.OtherUserEmail, "User"));
            var otherResponse = await otherClient.GetAsync(
                $"/api/Certificate/{seeded.CertificateId}/file");
            Assert.Equal(HttpStatusCode.NotFound, otherResponse.StatusCode);

            using var anonymousClient = CreateClient();
            var anonymousResponse = await anonymousClient.GetAsync(
                $"/api/Certificate/{seeded.CertificateId}/file");
            Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
        }
        finally
        {
            if (File.Exists(seeded.FilePath))
                File.Delete(seeded.FilePath);
        }
    }

    [Fact]
    public async Task LegacyPublicCertificatePath_IsBlocked()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/certificates/logo.png");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<SeededCertificate> SeedCertificateAsync()
    {
        const string otherUserEmail = "certificate-other@test.com";
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var owner = await userManager.FindByEmailAsync("user@test.com")
                    ?? throw new InvalidOperationException("Test certificate owner was not seeded.");
        var otherUser = await userManager.FindByEmailAsync(otherUserEmail);
        if (otherUser is null)
        {
            otherUser = new AppUser
            {
                DisplayName = "Other Certificate User",
                Email = otherUserEmail,
                UserName = otherUserEmail,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(otherUser, "IntegrationTest123!");
            Assert.True(createResult.Succeeded);
            var roleResult = await userManager.AddToRoleAsync(otherUser, "User");
            Assert.True(roleResult.Succeeded);
        }

        var dbContext = services.GetRequiredService<AppDbContext>();
        var certificateFileStorage = services.GetRequiredService<ICertificateFileStorage>();
        var courseId = await dbContext.Courses.Select(course => course.Id).FirstAsync();
        var quiz = new Quiz
        {
            Name = "Secure certificate quiz",
            Description = "Integration test quiz",
            CourseId = courseId,
            Questions = [],
            Progresses = [],
            Certificates = []
        };
        dbContext.Quizzes.Add(quiz);
        await dbContext.SaveChangesAsync();

        var fileName = $"certificate-secure-{Guid.NewGuid():N}.pdf";
        var filePath = certificateFileStorage.GetPrivateFilePath(fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllBytesAsync(filePath, PdfContent);

        var certificate = new Certificate
        {
            Name = "Secure certificate",
            Description = "Owner-only certificate",
            PdfUrl = fileName,
            UserId = owner.Id,
            QuizId = quiz.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Certificates.Add(certificate);
        await dbContext.SaveChangesAsync();

        return new SeededCertificate(certificate.Id, filePath, otherUserEmail);
    }

    private HttpClient CreateClient(string? token = null)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private sealed record SeededCertificate(
        int CertificateId,
        string FilePath,
        string OtherUserEmail);
}
