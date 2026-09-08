using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Policies;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Service.AppServices;

public sealed class AchievementService : IAchievementService
{
    private static readonly TimeZoneInfo PlatformTimeZone = ResolvePlatformTimeZone();

    private readonly IAchievementRepository _achievementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public AchievementService(
        IAchievementRepository achievementRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _achievementRepository = achievementRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public Task<AchievementDashboardDto> GetMineAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        return BuildDashboardAsync(userId, unlockEligible: false, cancellationToken);
    }

    public async Task<AchievementDashboardDto> CheckInAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var utcNow = _timeProvider.GetUtcNow();
        var activityDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(utcNow, PlatformTimeZone).DateTime);

        if (!await _achievementRepository.HasActivityDayAsync(
                userId,
                activityDate,
                cancellationToken))
        {
            await _unitOfWork.Repository<UserActivityDay>().AddAsync(
                new UserActivityDay
                {
                    UserId = userId,
                    ActivityDate = activityDate,
                    CreatedAt = utcNow.UtcDateTime
                },
                cancellationToken);
            await _unitOfWork.Complete(cancellationToken);
        }

        return await BuildDashboardAsync(userId, unlockEligible: true, cancellationToken);
    }

    private async Task<AchievementDashboardDto> BuildDashboardAsync(
        string userId,
        bool unlockEligible,
        CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(utcNow, PlatformTimeZone).DateTime);
        var learning = await _achievementRepository.GetLearningSnapshotAsync(
            userId,
            cancellationToken);
        var activityDates = await _achievementRepository.GetActivityDatesAsync(
            userId,
            cancellationToken);
        var streaks = AchievementPolicy.CalculateStreaks(activityDates, today);
        var totalPoints = learning.CompletedCourses * UserRankPolicy.PointsPerCompletedCourse
            + learning.Certificates * UserRankPolicy.PointsPerCertificate
            + UserRankPolicy.CalculateQuizAverageBonus(learning.AverageQuizScore);
        var metrics = new AchievementMetrics(
            learning.CompletedLessons,
            learning.CompletedCourses,
            learning.HighScoreQuizzes,
            totalPoints,
            streaks.Current,
            streaks.Longest);
        var unlocked = (await _achievementRepository.GetUnlockedAsync(userId, cancellationToken))
            .ToDictionary(item => item.Key, item => item.Value);
        var newlyUnlocked = new List<AchievementCode>();

        if (unlockEligible)
        {
            foreach (var code in AchievementPolicy.EligibleAchievements(metrics))
            {
                if (unlocked.ContainsKey(code))
                    continue;

                var unlockedAt = utcNow.UtcDateTime;
                await _unitOfWork.Repository<UserAchievement>().AddAsync(
                    new UserAchievement
                    {
                        UserId = userId,
                        Code = code,
                        UnlockedAtUtc = unlockedAt,
                        CreatedAt = unlockedAt
                    },
                    cancellationToken);
                unlocked[code] = unlockedAt;
                newlyUnlocked.Add(code);
            }

            if (newlyUnlocked.Count > 0)
                await _unitOfWork.Complete(cancellationToken);
        }

        var items = AchievementPolicy.Definitions
            .Select(definition =>
            {
                var isUnlocked = unlocked.TryGetValue(definition.Code, out var unlockedAt);
                return new AchievementItemDto
                {
                    Code = definition.Code.ToString(),
                    Title = definition.Title,
                    Description = definition.Description,
                    IconKey = definition.IconKey,
                    IsUnlocked = isUnlocked,
                    UnlockedAtUtc = isUnlocked ? unlockedAt : null,
                    CurrentValue = Math.Min(
                        AchievementPolicy.CurrentValue(definition, metrics),
                        definition.Target),
                    Target = definition.Target
                };
            })
            .ToList();

        return new AchievementDashboardDto
        {
            CurrentStreak = streaks.Current,
            LongestStreak = streaks.Longest,
            UnlockedCount = unlocked.Count,
            TotalCount = AchievementPolicy.Definitions.Count,
            NewlyUnlocked = newlyUnlocked.Select(code => code.ToString()).ToList(),
            Achievements = items
        };
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("معرف المستخدم مطلوب", nameof(userId));
    }

    private static TimeZoneInfo ResolvePlatformTimeZone()
    {
        foreach (var timeZoneId in new[] { "Africa/Cairo", "Egypt Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}
