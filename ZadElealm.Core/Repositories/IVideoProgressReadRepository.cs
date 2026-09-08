namespace ZadElealm.Core.Repositories;

public interface IVideoProgressReadRepository
{
    Task<CourseProgressSummaryReadModel?> GetCourseSummaryAsync(
        string userId,
        int courseId,
        CancellationToken cancellationToken = default);
}

public sealed record CourseProgressSummaryReadModel(
    bool IsEnrolled,
    int TotalVideos,
    int CompletedVideos);
