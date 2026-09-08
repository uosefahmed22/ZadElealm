using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Repositories;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Repositories;

public sealed class RatingReadRepository : IRatingReadRepository
{
    private readonly AppDbContext _dbContext;

    public RatingReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RatingSummaryReadModel> GetSummaryAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var summary = await _dbContext.Ratings
            .AsNoTracking()
            .Where(rating => rating.courseId == courseId)
            .GroupBy(_ => 1)
            .Select(group => new RatingSummaryReadModel(
                group.Count(),
                group.Average(rating => (double)rating.Value)))
            .FirstOrDefaultAsync(cancellationToken);

        return summary ?? new RatingSummaryReadModel(0, 0d);
    }
}
