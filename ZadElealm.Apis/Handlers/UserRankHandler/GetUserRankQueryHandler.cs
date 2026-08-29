using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.UserRank;
using ZadElealm.Service.Mappers;

namespace ZadElealm.Apis.Handlers.UserRankHandler
{
    public class GetUserRankQueryHandler : IRequestHandler<GetUserRankQuery, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserRankQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            var userrankdto = userRank.ToDto();

            return new ApiDataResponse(200, userrankdto, "User rank found");
        }
    }
}