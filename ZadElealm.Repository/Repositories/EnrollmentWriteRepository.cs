using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Repositories;

public sealed class EnrollmentWriteRepository : IEnrollmentWriteRepository
{
    private readonly AppDbContext _dbContext;

    public EnrollmentWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Enrollment?> FindIncludingDeletedAsync(
        int courseId,
        string userId,
        CancellationToken cancellationToken = default)
        => _dbContext.Enrollments
            .IgnoreQueryFilters()
            .Where(enrollment => enrollment.CourseId == courseId &&
                enrollment.AppUserId == userId)
            .OrderByDescending(enrollment => !enrollment.IsDeleted)
            .ThenByDescending(enrollment => enrollment.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
