using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Core.Repositories;

public interface IEnrollmentReadRepository
{
    Task<IReadOnlyList<EnrolledCourseProgressReadModel>> GetUserCoursesWithProgressAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
