using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests;

public sealed class RankDashboardTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly ZadElealmApiFactory _factory;

    public RankDashboardTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PublicRankEndpoints_EnforceAuthorizationAndPagingBounds()
    {
        using var anonymous = CreateClient();

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/Rank/calculate")).StatusCode);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await anonymous.GetAsync("/api/Rank/top?take=101")).StatusCode);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await anonymous.GetAsync("/api/Rank/leaderboard?page=0&pageSize=10")).StatusCode);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await anonymous.GetAsync("/api/Rank/leaderboard?page=1&pageSize=101")).StatusCode);
    }

    [Fact]
    public async Task DashboardRequiresAStudent_ValidatesTake_AndReturnsSafeLiveRanking()
    {
        using var anonymous = CreateClient();
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/Rank/dashboard")).StatusCode);

        using var student = CreateAuthenticatedClient();
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await student.GetAsync("/api/Rank/dashboard?take=2")).StatusCode);

        await SeedLeaderAsync();

        var response = await student.GetAsync("/api/Rank/dashboard?take=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<RankDashboardEnvelope>();
        Assert.NotNull(envelope);
        Assert.Equal(0, envelope.Data.CurrentUser.TotalPoints);
        Assert.Equal("Bronze", envelope.Data.CurrentUser.Rank);
        Assert.Equal(10, envelope.Data.CurrentUser.PointsBreakdown.PointsPerCompletedCourse);
        Assert.Equal(20, envelope.Data.CurrentUser.PointsBreakdown.PointsPerCertificate);
        Assert.Equal(50, envelope.Data.CurrentUser.PointsBreakdown.QuizAverageContributionPercentage);
        Assert.Equal(0, envelope.Data.CurrentUser.PointsBreakdown.CompletedCoursesPoints);
        Assert.Equal(0, envelope.Data.CurrentUser.PointsBreakdown.CertificatesPoints);
        Assert.Equal(0, envelope.Data.CurrentUser.PointsBreakdown.QuizAverageBonusPoints);
        Assert.True(envelope.Data.CurrentUser.LastUpdated > DateTime.UtcNow.AddMinutes(-1));

        Assert.Collection(
            envelope.Data.Tiers,
            tier => Assert.Equal(new RankTier("Bronze", 0, 99), tier),
            tier => Assert.Equal(new RankTier("Silver", 100, 299), tier),
            tier => Assert.Equal(new RankTier("Gold", 300, 599), tier),
            tier => Assert.Equal(new RankTier("Platinum", 600, 999), tier),
            tier => Assert.Equal(new RankTier("Diamond", 1000, null), tier));

        var leader = Assert.Single(envelope.Data.Leaders, item => item.DisplayName == "متصدر الاختبار");
        Assert.Equal(1, leader.Position);
        Assert.Equal(500, leader.TotalPoints);
        Assert.Equal("Gold", leader.Rank);
        Assert.False(leader.IsCurrentUser);

        var current = Assert.Single(envelope.Data.Leaders, item => item.IsCurrentUser);
        Assert.Equal("Integration Test User", current.DisplayName);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUser = await dbContext.Users.SingleAsync(user => user.Email == "user@test.com");
        var storedRank = await dbContext.userRanks.SingleAsync(rank => rank.UserId == currentUser.Id);
        Assert.Equal(0, storedRank.TotalPoints);
        Assert.Equal(UserRankEnum.Bronze, storedRank.Rank);
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateToken("User"));
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

    private async Task SeedLeaderAsync()
    {
        const string email = "rank-leader@test.com";
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new AppUser
            {
                DisplayName = "متصدر الاختبار",
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };
            Assert.True((await userManager.CreateAsync(
                user,
                ZadElealmApiFactory.TestUserPassword)).Succeeded);
        }

        if (!await dbContext.userRanks.AnyAsync(rank => rank.UserId == user.Id))
        {
            dbContext.userRanks.Add(new UserRank
            {
                UserId = user.Id,
                TotalPoints = 500,
                Rank = UserRankEnum.Gold,
                CompletedCoursesCount = 12,
                CertificatesCount = 3,
                AverageQuizScore = 94,
                LastUpdated = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }
    }

    private sealed record RankDashboardEnvelope(int StatusCode, RankDashboardData Data);
    private sealed record RankDashboardData(
        StudentRankSummary CurrentUser,
        List<LeaderboardEntry> Leaders,
        List<RankTier> Tiers);
    private sealed record StudentRankSummary(
        int TotalPoints,
        string Rank,
        int CompletedCoursesCount,
        int CertificatesCount,
        double AverageQuizScore,
        DateTime LastUpdated,
        RankPointsBreakdown PointsBreakdown);
    private sealed record RankPointsBreakdown(
        int CompletedCoursesPoints,
        int CertificatesPoints,
        int QuizAverageBonusPoints,
        int PointsPerCompletedCourse,
        int PointsPerCertificate,
        int QuizAverageContributionPercentage);
    private sealed record RankTier(string Rank, int MinimumPoints, int? MaximumPoints);
    private sealed record LeaderboardEntry(
        int Position,
        string DisplayName,
        string? ImageUrl,
        int TotalPoints,
        string Rank,
        int CompletedCoursesCount,
        bool IsCurrentUser);
}
