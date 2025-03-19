using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Review;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.Review
{
    public class DeleteReplyreviewCommandHandler : BaseCommandHandler<DeleteReplyreviewCommand, ApiResponse>
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public DeleteReplyreviewCommandHandler(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public override async Task<ApiResponse> Handle(DeleteReplyreviewCommand request, CancellationToken cancellationToken)
        {
            var existingReply = await _unitOfWork.Repository<Core.Models.Reply>()
                .GetEntityAsync(request.ReplyId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (existingReply == null || user == null)
            {
                return new ApiResponse(404, "Reply not found");
            }

            if (existingReply.AppUserId != request.UserId)
            {
                return new ApiResponse(403, "You are not authorized to delete this reply");
            }

            _unitOfWork.Repository<Core.Models.Reply>().Delete(existingReply);
            await _unitOfWork.Complete();
            return new ApiResponse(200, "Reply deleted successfully");
        }
    }
}