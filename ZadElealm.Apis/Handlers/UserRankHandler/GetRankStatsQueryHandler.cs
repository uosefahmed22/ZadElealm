using MediatR;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class GetRankStatsQueryHandler : IRequestHandler<GetRankStatsQuery, Dictionary<UserRankEnum, int>>
    {
        private readonly IUserRankReadRepository _userRankReadRepository;

        public GetRankStatsQueryHandler(IUserRankReadRepository userRankReadRepository)
        {
            _userRankReadRepository = userRankReadRepository;
        }

        public async Task<Dictionary<UserRankEnum, int>> Handle(
            GetRankStatsQuery request,
            CancellationToken cancellationToken)
        {
            var counts = await _userRankReadRepository
                .GetCountsByRankAsync(cancellationToken);

            return Enum.GetValues<UserRankEnum>()
                .ToDictionary(rank => rank, rank => counts.GetValueOrDefault(rank));
        }
    }
}
