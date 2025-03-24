using AutoMapper;
using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.UserRank;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class GetUserRankQueryHandler : IRequestHandler<GetUserRankQuery, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserRankQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiDataResponse> Handle(GetUserRankQuery request, CancellationToken cancellationToken)
        {
            var spec = new UserRankWithUserSpecification(request.UserId);
            var userRank = await _unitOfWork.Repository<UserRank>()
                .GetEntityWithSpecNoTrackingAsync(spec);

            if (userRank == null)
            {
                return new ApiDataResponse(200, "المستخدم ليس لديه تصنيف حاليًا");
            }

            var userrankdto = _mapper.Map<UserRankDto>(userRank);

            return new ApiDataResponse(200, userrankdto, "User rank found");
        }
    }
}