using AutoMapper;
using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Review;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Review;

namespace ZadElealm.Apis.Handlers.ReplyCommandHandler
{
    public class GetReviewRepliesQueryHandler : BaseQueryHandler<GetReviewRepliesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetReviewRepliesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async override Task<ApiResponse> Handle(GetReviewRepliesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var review = await _unitOfWork.Repository<Core.Models.Review>()
                    .GetEntityWithNoTrackingAsync(request.ReviewId);

                if (review == null)
                    return new ApiResponse(404, "المراجعة غير موجودة");

                var spec = new RepliesWithUserSpecification(request.ReviewId);
                var replies = await _unitOfWork.Repository<Reply>()
                    .GetAllWithSpecNoTrackingAsync(spec);

                if (!replies.Any())
                    return new ApiResponse(404, "لا توجد ردود لهذه المراجعة");

                var repliesDto = _mapper.Map<IReadOnlyList<ReplyDto>>(replies);

                return new ApiDataResponse(200, repliesDto, "تم جلب الردود بنجاح");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ أثناء جلب الردود");
            }
        }
    }
}
