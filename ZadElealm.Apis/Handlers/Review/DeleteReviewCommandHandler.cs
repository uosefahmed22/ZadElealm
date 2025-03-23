using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Review;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.Review
{
    public class DeleteReviewCommandHandler : BaseCommandHandler<DeleteReviewCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public DeleteReviewCommandHandler(IUnitOfWork unitOfWork,UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public override async Task<ApiResponse> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var existingReview =
                await _unitOfWork.Repository<Core.Models.Review>().GetEntityAsync(request.ReviewId);
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (existingReview == null || user == null)
            {
                return new ApiResponse(404, "لم يتم العثور على المراجعة");
            }

            if (existingReview.AppUserId != request.UserId)
            {
                return new ApiResponse(403, "أنت غير مخول لحذف هذه المراجعة");
            }

            _unitOfWork.Repository<Core.Models.Review>().Delete(existingReview);
            await _unitOfWork.Complete();
            return new ApiResponse(200, "تم حذف المراجعة بنجاح");
        }
    }
}
