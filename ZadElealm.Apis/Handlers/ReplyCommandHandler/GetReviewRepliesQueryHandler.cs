using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Review;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Review;

namespace ZadElealm.Apis.Handlers.ReplyCommandHandler
{
    public class GetReviewRepliesQueryHandler : BaseQueryHandler<GetReviewRepliesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetReviewRepliesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async override Task<ApiResponse> Handle(GetReviewRepliesQuery request, CancellationToken cancellationToken)
        {
            var review = await _unitOfWork.Repository<Core.Models.Review>()
                .GetEntityWithNoTrackingAsync(request.ReviewId);

            if (review == null)
                return new ApiResponse(404, "المراجعة غير موجودة");

            var spec = new RepliesWithUserSpecification(request.ReviewId);
            var replies = await _unitOfWork.Repository<Reply>()
                .GetAllWithSpecNoTrackingAsync(spec);

            var repliesDto = replies.ToDtos(request.UserId);

            return new ApiDataResponse(200, repliesDto, "تم جلب الردود بنجاح");
        }
    }
}
