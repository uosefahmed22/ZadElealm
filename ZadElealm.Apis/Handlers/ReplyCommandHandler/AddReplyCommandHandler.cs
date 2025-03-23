using ZadElealm.Apis.Commands.Review;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.ReplyCommandHandler
{
    public class AddReplyCommandHandler : BaseCommandHandler<AddReplyCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddReplyCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(AddReplyCommand request, CancellationToken cancellationToken)
        {
            var review = await _unitOfWork.Repository<Core.Models.Review>().GetEntityAsync(request.ReviewId);
            if (review == null)
                return new ApiResponse(404, "المراجعة غير موجودة");

            if (string.IsNullOrWhiteSpace(request.ReplyText))
                return new ApiResponse(400, "نص الرد مطلوب");

            var reply = new Reply
            {
                Text = request.ReplyText.Trim(),
                ReviewId = request.ReviewId,
                AppUserId = request.UserId
            };

            await _unitOfWork.Repository<Reply>().AddAsync(reply);
            await _unitOfWork.Complete();

            return new ApiResponse(200, "تم إضافة الرد بنجاح");
        }
    }
}