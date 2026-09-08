using ZadElealm.Core.Enums;
using ZadElealm.Core.Policies;
using Xunit;

namespace ZadElealm.UnitTests.Services;

public sealed class AchievementPolicyTests
{
    [Fact]
    public void CalculateStreaks_IgnoresDuplicateDaysAndCalculatesCurrentAndLongest()
    {
        var today = new DateOnly(2026, 9, 7);
        var dates = new[]
        {
            today.AddDays(-6),
            today.AddDays(-5),
            today.AddDays(-4),
            today.AddDays(-3),
            today.AddDays(-2),
            today.AddDays(-1),
            today,
            today
        };

        var result = AchievementPolicy.CalculateStreaks(dates, today);

        Assert.Equal(7, result.Current);
        Assert.Equal(7, result.Longest);
    }

    [Fact]
    public void CalculateStreaks_KeepsYesterdayStreakActiveUntilTodayEnds()
    {
        var today = new DateOnly(2026, 9, 7);

        var result = AchievementPolicy.CalculateStreaks(
            new[] { today.AddDays(-3), today.AddDays(-2), today.AddDays(-1) },
            today);

        Assert.Equal(3, result.Current);
        Assert.Equal(3, result.Longest);
    }

    [Fact]
    public void CalculateStreaks_ResetsCurrentAfterMissingAFullDayButKeepsLongest()
    {
        var today = new DateOnly(2026, 9, 7);

        var result = AchievementPolicy.CalculateStreaks(
            new[] { today.AddDays(-5), today.AddDays(-4), today.AddDays(-3) },
            today);

        Assert.Equal(0, result.Current);
        Assert.Equal(3, result.Longest);
    }

    [Fact]
    public void EligibleAchievements_UsesPublishedBoundariesWithoutUnlockingHigherGoals()
    {
        var metrics = new AchievementMetrics(
            CompletedLessons: 1,
            CompletedCourses: 5,
            HighScoreQuizzes: 3,
            TotalPoints: 300,
            CurrentStreak: 7,
            LongestStreak: 7);

        var achievements = AchievementPolicy.EligibleAchievements(metrics);

        Assert.Contains(AchievementCode.FirstLesson, achievements);
        Assert.Contains(AchievementCode.FirstCourse, achievements);
        Assert.Contains(AchievementCode.FiveCourses, achievements);
        Assert.Contains(AchievementCode.SevenDayStreak, achievements);
        Assert.Contains(AchievementCode.QuizExcellence, achievements);
        Assert.Contains(AchievementCode.GoldRank, achievements);
        Assert.DoesNotContain(AchievementCode.TenCourses, achievements);
        Assert.DoesNotContain(AchievementCode.ThirtyDayStreak, achievements);
        Assert.DoesNotContain(AchievementCode.PlatinumRank, achievements);
    }
}
