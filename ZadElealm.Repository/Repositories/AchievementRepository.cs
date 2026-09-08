using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Repositories;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Repositories;

public sealed class AchievementRepository : IAchievementRepository
{
    private readonly AppDbContext _dbContext;

    public AchievementRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AchievementLearningSnapshot> GetLearningSnapshotAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var completedProgresses = await _dbContext.Progresses
            .AsNoTracking()
            .Where(progress => progress.AppUserId == userId && progress.IsCompleted)
            .Select(progress => new
            {
                progress.Score,
                progress.Quiz.CourseId
            })
            .ToListAsync(cancellationToken);

        var completedLessons = await _dbContext.VideoProgresses
            .AsNoTracking()
            .CountAsync(
                progress => progress.UserId == userId && progress.IsCompleted,
                cancellationToken);

        var certificates = await _dbContext.Certificates
            .AsNoTracking()
            .CountAsync(certificate => certificate.UserId == userId, cancellationToken);

        return new AchievementLearningSnapshot(
            completedLessons,
            completedProgresses.Select(progress => progress.CourseId).Distinct().Count(),
            certificates,
            completedProgresses.Count,
            completedProgresses.Count(progress => progress.Score >= 90),
            completedProgresses.Count == 0
                ? 0
                : completedProgresses.Average(progress => progress.Score));
    }

    public async Task<IReadOnlyList<DateOnly>> GetActivityDatesAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserActivityDays
            .AsNoTracking()
            .Where(activity => activity.UserId == userId)
            .OrderBy(activity => activity.ActivityDate)
            .Select(activity => activity.ActivityDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<AchievementCode, DateTime>> GetUnlockedAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserAchievements
            .AsNoTracking()
            .Where(achievement => achievement.UserId == userId)
            .ToDictionaryAsync(
                achievement => achievement.Code,
                achievement => achievement.UnlockedAtUtc,
                cancellationToken);
    }

    public Task<bool> HasActivityDayAsync(
        string userId,
        DateOnly activityDate,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserActivityDays.AnyAsync(
            activity => activity.UserId == userId && activity.ActivityDate == activityDate,
            cancellationToken);
    }
}
