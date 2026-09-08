using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Core.Repositories;

public interface IEnrollmentReadRepository
{
    Task<IReadOnlyList<EnrolledCourseProgressReadModel>> GetUserCoursesWithProgressAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<CourseEnrollmentSummaryReadModel> GetCourseEnrollmentSummaryAsync(
        int courseId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<int>> GetCompletedCourseCategoryIdsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
