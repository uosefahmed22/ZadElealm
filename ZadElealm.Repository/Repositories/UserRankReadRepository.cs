using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Repositories;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Repositories;

public sealed class UserRankReadRepository : IUserRankReadRepository
{
    private readonly AppDbContext _dbContext;

    public UserRankReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<UserRankEnum, int>> GetCountsByRankAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.userRanks
            .AsNoTracking()
            .GroupBy(userRank => userRank.Rank)
            .Select(group => new { Rank = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Rank, item => item.Count, cancellationToken);
    }
}
