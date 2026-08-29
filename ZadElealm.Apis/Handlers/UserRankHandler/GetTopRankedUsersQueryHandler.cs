using MediatR;
using System.Collections.Generic;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.UserRank;
using ZadElealm.Service.Mappers;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class GetTopRankedUsersQueryHandler : IRequestHandler<GetTopRankedUsersQuery, List<UserRankDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTopRankedUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UserRankDto>> Handle(GetTopRankedUsersQuery request, CancellationToken cancellationToken)
        {
            var spec = new TopRankedUsersSpecification(request.Take);
            var topUsers = await _unitOfWork.Repository<UserRank>()
                .GetAllWithSpecNoTrackingAsync(spec);

            return topUsers.ToDtos();
        }
    }
}
