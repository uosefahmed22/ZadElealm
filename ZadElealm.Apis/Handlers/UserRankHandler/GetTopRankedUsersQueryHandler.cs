using AutoMapper;
using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.UserRank;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class GetTopRankedUsersQueryHandler : IRequestHandler<GetTopRankedUsersQuery, List<UserRankDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTopRankedUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<UserRankDto>> Handle(GetTopRankedUsersQuery request, CancellationToken cancellationToken)
        {
            var spec = new TopRankedUsersSpecification(request.Take);
            var topUsers = await _unitOfWork.Repository<UserRank>()
                .GetAllWithSpecNoTrackingAsync(spec);

            return _mapper.Map<List<UserRankDto>>(topUsers);
        }
    }
}
