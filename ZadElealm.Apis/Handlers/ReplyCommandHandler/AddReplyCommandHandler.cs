using ZadElealm.Apis.Commands.Review;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

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

            var enrollment = await _unitOfWork.Repository<Enrollment>()
                .GetEntityWithSpecNoTrackingAsync(
                    new EnrollmentExistsSpecification(review.CourseId, request.UserId));
            if (enrollment == null)
                return new ApiResponse(400, "يجب التسجيل في الدورة أولاً قبل إضافة رد");

            var replyText = request.ReplyText?.Trim();
            if (string.IsNullOrWhiteSpace(replyText) || replyText.Length < 2 || replyText.Length > 500)
                return new ApiResponse(400, "نص الرد يجب أن يكون بين حرفين و500 حرف");

            var reply = new Reply
            {
                Text = replyText,
                ReviewId = request.ReviewId,
                AppUserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Reply>().AddAsync(reply);
            await _unitOfWork.Complete();

            return new ApiResponse(200, "تم إضافة الرد بنجاح");
        }
    }
}
