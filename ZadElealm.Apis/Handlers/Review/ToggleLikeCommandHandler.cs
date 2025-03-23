using ZadElealm.Apis.Commands.Review;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Review;

namespace ZadElealm.Apis.Handlers.Review
{
    public class ToggleLikeCommandHandler : BaseCommandHandler<ToggleLikeCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ToggleLikeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
        {
            var review = await _unitOfWork.Repository<Core.Models.Review>()
                .GetEntityWithNoTrackingAsync(request.ReviewId);
            if (review == null)
                return new ApiResponse(404, "المراجعة غير موجودة");

            var spec = new ReviewLikeSpecification(request.ReviewId, request.UserId);
            var existingLike = await _unitOfWork.Repository<ReviewLike>()
                .GetEntityWithSpecAsync(spec);

            if (existingLike != null)
            {
                _unitOfWork.Repository<ReviewLike>().Delete(existingLike);
                await _unitOfWork.Complete();
                return new ApiResponse(200, "تم إلغاء الإعجاب");
            }
            else
            {
                var like = new ReviewLike
                {
                    ReviewId = request.ReviewId,
                    AppUserId = request.UserId
                };

                await _unitOfWork.Repository<ReviewLike>().AddAsync(like);
                await _unitOfWork.Complete();
                return new ApiResponse(200, "تم إضافة الإعجاب");
            }
        }
    }
}