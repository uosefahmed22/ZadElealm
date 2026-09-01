using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Review;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.ReplyCommandHandler
{
    public class DeleteReplyreviewCommandHandler : BaseCommandHandler<DeleteReplyreviewCommand, ApiResponse>
    {

        private readonly IUnitOfWork _unitOfWork;
        public DeleteReplyreviewCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public override async Task<ApiResponse> Handle(DeleteReplyreviewCommand request, CancellationToken cancellationToken)
        {
            var existingReply = await _unitOfWork.Repository<Core.Models.Reply>()
                .GetEntityAsync(request.ReplyId);

            if (existingReply == null)
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
