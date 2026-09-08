using ZadElealm.Core.Enums;

namespace ZadElealm.Core.Repositories;

public interface IAchievementRepository
{
    Task<AchievementLearningSnapshot> GetLearningSnapshotAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DateOnly>> GetActivityDatesAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<AchievementCode, DateTime>> GetUnlockedAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<bool> HasActivityDayAsync(
        string userId,
        DateOnly activityDate,
        CancellationToken cancellationToken);
}

public sealed record AchievementLearningSnapshot(
    int CompletedLessons,
    int CompletedCourses,
    int Certificates,
    int CompletedQuizzes,
    int HighScoreQuizzes,
    double AverageQuizScore);
