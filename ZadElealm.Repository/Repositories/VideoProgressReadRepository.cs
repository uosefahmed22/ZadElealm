using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Repositories;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Repositories;

public sealed class VideoProgressReadRepository : IVideoProgressReadRepository
{
    private readonly AppDbContext _dbContext;

    public VideoProgressReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CourseProgressSummaryReadModel?> GetCourseSummaryAsync(
        string userId,
        int courseId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Id == courseId)
            .Select(course => new CourseProgressSummaryReadModel(
                _dbContext.Enrollments.Any(enrollment =>
                    enrollment.CourseId == courseId &&
                    enrollment.AppUserId == userId),
                _dbContext.Videos.Count(video => video.CourseId == courseId),
                _dbContext.VideoProgresses.Count(progress =>
                    progress.UserId == userId &&
                    progress.CourseId == courseId &&
                    progress.IsCompleted)))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
