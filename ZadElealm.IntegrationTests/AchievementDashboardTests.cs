using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;
using Xunit;

namespace ZadElealm.IntegrationTests;

public sealed class AchievementDashboardTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly ZadElealmApiFactory _factory;

    public AchievementDashboardTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AchievementEndpoints_RequireAStudent()
    {
        using var client = CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Achievements/mine")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/api/Achievements/check-in", null)).StatusCode);
    }

    [Fact]
    public async Task CheckIn_IsIdempotentForTheSameDay_AndReturnsTheFullCatalog()
    {
        var email = $"achievement-{Guid.NewGuid():N}@test.com";
        var userId = await CreateStudentAsync(email);
        using var client = CreateAuthenticatedClient(email);

        var first = await client.PostAsync("/api/Achievements/check-in", null);
        var second = await client.PostAsync("/api/Achievements/check-in", null);

        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();
        var response = await second.Content.ReadFromJsonAsync<AchievementEnvelope>();
        Assert.NotNull(response);
        Assert.Equal(1, response.Data.CurrentStreak);
        Assert.Equal(11, response.Data.TotalCount);
        Assert.Equal(0, response.Data.UnlockedCount);
        Assert.Equal(11, response.Data.Achievements.Count);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await dbContext.UserActivityDays.CountAsync(day => day.UserId == userId));
    }

    [Fact]
    public async Task CheckIn_UnlocksSevenDayStreakOnceAndReturnsProgress()
    {
        var email = $"streak-{Guid.NewGuid():N}@test.com";
        var userId = await CreateStudentAsync(email);
        var today = CurrentPlatformDate();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            for (var daysAgo = 6; daysAgo >= 1; daysAgo--)
            {
                dbContext.UserActivityDays.Add(new UserActivityDay
                {
                    UserId = userId,
                    ActivityDate = today.AddDays(-daysAgo),
                    CreatedAt = DateTime.UtcNow.AddDays(-daysAgo)
                });
            }
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateAuthenticatedClient(email);
        var first = await client.PostAsync("/api/Achievements/check-in", null);
        var second = await client.PostAsync("/api/Achievements/check-in", null);

        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();
        var firstResponse = await first.Content.ReadFromJsonAsync<AchievementEnvelope>();
        var secondResponse = await second.Content.ReadFromJsonAsync<AchievementEnvelope>();
        Assert.NotNull(firstResponse);
        Assert.NotNull(secondResponse);
        Assert.Equal(7, firstResponse.Data.CurrentStreak);
        Assert.Contains(nameof(AchievementCode.SevenDayStreak), firstResponse.Data.NewlyUnlocked);
        Assert.Empty(secondResponse.Data.NewlyUnlocked);

        var streak = Assert.Single(
            secondResponse.Data.Achievements,
            achievement => achievement.Code == nameof(AchievementCode.SevenDayStreak));
        Assert.True(streak.IsUnlocked);
        Assert.Equal(7, streak.CurrentValue);
        Assert.Equal(7, streak.Target);

        using var verificationScope = _factory.Services.CreateScope();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(
            1,
            await verificationDbContext.UserAchievements.CountAsync(achievement =>
                achievement.UserId == userId &&
                achievement.Code == AchievementCode.SevenDayStreak));
    }

    [Fact]
    public async Task CheckIn_UnlocksCourseAchievementFromCompletedQuizData()
    {
        var email = $"course-achievement-{Guid.NewGuid():N}@test.com";
        var userId = await CreateStudentAsync(email);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var courseId = await dbContext.Courses.Select(course => course.Id).FirstAsync();
            var quiz = new Quiz
            {
                Name = "اختبار إنجاز",
                Description = "اختبار مخصص للتحقق من الإنجازات",
                CourseId = courseId,
                Questions = []
            };
            dbContext.Quizzes.Add(quiz);
            await dbContext.SaveChangesAsync();

            dbContext.Progresses.Add(new Progress
            {
                AppUserId = userId,
                QuizId = quiz.Id,
                Score = 95,
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            });
            dbContext.Certificates.Add(new Certificate
            {
                UserId = userId,
                QuizId = quiz.Id,
                Name = "شهادة الإنجاز",
                Description = "شهادة اختبار الإنجازات",
                PdfUrl = "achievement-test.pdf",
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        using var client = CreateAuthenticatedClient(email);
        var response = await client.PostAsync("/api/Achievements/check-in", null);

        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<AchievementEnvelope>();
        Assert.NotNull(envelope);
        Assert.Contains(nameof(AchievementCode.FirstCourse), envelope.Data.NewlyUnlocked);
        Assert.DoesNotContain(nameof(AchievementCode.QuizExcellence), envelope.Data.NewlyUnlocked);
        Assert.DoesNotContain(nameof(AchievementCode.SilverRank), envelope.Data.NewlyUnlocked);

        var firstCourse = Assert.Single(
            envelope.Data.Achievements,
            achievement => achievement.Code == nameof(AchievementCode.FirstCourse));
        Assert.True(firstCourse.IsUnlocked);
        Assert.Equal(1, firstCourse.CurrentValue);
    }

    private async Task<string> CreateStudentAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = new AppUser
        {
            DisplayName = "طالب الإنجازات",
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };
        Assert.True((await userManager.CreateAsync(user, ZadElealmApiFactory.TestUserPassword)).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(user, "User")).Succeeded);
        return user.Id;
    }

    private HttpClient CreateAuthenticatedClient(string email)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateTokenForEmail(email, "User"));
        return client;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static DateOnly CurrentPlatformDate()
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }
        catch (TimeZoneNotFoundException)
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        }

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).DateTime);
    }

    private sealed record AchievementEnvelope(int StatusCode, AchievementDashboard Data);
    private sealed record AchievementDashboard(
        int CurrentStreak,
        int LongestStreak,
        int UnlockedCount,
        int TotalCount,
        List<string> NewlyUnlocked,
        List<AchievementItem> Achievements);
    private sealed record AchievementItem(
        string Code,
        bool IsUnlocked,
        int CurrentValue,
        int Target);
}
