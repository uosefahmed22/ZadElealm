using ZadElealm.Core.Models;

namespace ZadElealm.Core.Repositories;

public interface IEnrollmentWriteRepository
{
    Task<Enrollment?> FindIncludingDeletedAsync(
        int courseId,
        string userId,
        CancellationToken cancellationToken = default);
}
