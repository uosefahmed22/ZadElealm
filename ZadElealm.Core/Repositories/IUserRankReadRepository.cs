using ZadElealm.Core.Enums;

namespace ZadElealm.Core.Repositories;

public interface IUserRankReadRepository
{
    Task<IReadOnlyDictionary<UserRankEnum, int>> GetCountsByRankAsync(
        CancellationToken cancellationToken = default);
}
