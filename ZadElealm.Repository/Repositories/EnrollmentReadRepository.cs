using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Repositories;

public sealed class EnrollmentReadRepository : IEnrollmentReadRepository
{
    private readonly AppDbContext _dbContext;

    public EnrollmentReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EnrolledCourseProgressReadModel>>
        GetUserCoursesWithProgressAsync(
            string userId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.AppUserId == userId)
            .OrderByDescending(enrollment => enrollment.CreatedAt)
            .Select(enrollment => new EnrolledCourseProgressReadModel
            {
                CourseId = enrollment.Course.Id,
                CourseName = enrollment.Course.Name,
                CourseDescription = enrollment.Course.Description,
                Author = enrollment.Course.Author,
                CourseLanguage = enrollment.Course.CourseLanguage,
                CourseVideosCount = enrollment.Course.CourseVideosCount,
                Rating = enrollment.Course.rating,
                ImageUrl = enrollment.Course.ImageUrl,
                CreatedAt = enrollment.Course.CreatedAt,
                CategoryId = enrollment.Course.Category.Id,
                CategoryName = enrollment.Course.Category.Name,
                CategoryDescription = enrollment.Course.Category.Description,
                CategoryImageUrl = enrollment.Course.Category.ImageUrl,
                TotalVideos = _dbContext.Videos.Count(video =>
                    video.CourseId == enrollment.CourseId),
                CompletedVideos = _dbContext.VideoProgresses.Count(progress =>
                    progress.UserId == userId &&
                    progress.CourseId == enrollment.CourseId &&
                    progress.IsCompleted)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CourseEnrollmentSummaryReadModel> GetCourseEnrollmentSummaryAsync(
        int courseId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var summary = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.CourseId == courseId)
            .GroupBy(_ => 1)
            .Select(group => new CourseEnrollmentSummaryReadModel(
                group.Count(),
                group.Any(enrollment => enrollment.AppUserId == userId)))
            .FirstOrDefaultAsync(cancellationToken);

        return summary ?? new CourseEnrollmentSummaryReadModel(0, false);
    }

    public async Task<IReadOnlySet<int>> GetCompletedCourseCategoryIdsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var categoryIds = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.AppUserId == userId)
            .Where(enrollment => _dbContext.Videos.Any(video =>
                video.CourseId == enrollment.CourseId))
            .Where(enrollment => !_dbContext.Videos
                .Where(video => video.CourseId == enrollment.CourseId)
                .Any(video => !_dbContext.VideoProgresses.Any(progress =>
                    progress.UserId == userId &&
                    progress.VideoId == video.Id &&
                    progress.IsCompleted)))
            .Select(enrollment => enrollment.Course.CategoryId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return categoryIds.ToHashSet();
    }
}
