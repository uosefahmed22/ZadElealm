using MediatR;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class GetRankStatsQueryHandler : IRequestHandler<GetRankStatsQuery, Dictionary<UserRankEnum, int>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRankStatsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Dictionary<UserRankEnum, int>> Handle(
            GetRankStatsQuery request,
            CancellationToken cancellationToken)
        {
            var userRanks = await _unitOfWork.Repository<UserRank>().GetAllWithNoTrackingAsync();
            var counts = userRanks
                .GroupBy(userRank => userRank.Rank)
                .ToDictionary(group => group.Key, group => group.Count());

            return Enum.GetValues<UserRankEnum>()
                .ToDictionary(rank => rank, rank => counts.GetValueOrDefault(rank));
        }
    }
}
