using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Review;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Review;

namespace ZadElealm.Apis.Handlers.Review
{
    public class AddReviewCommandHandler : BaseCommandHandler<AddReviewCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddReviewCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(AddReviewCommand request, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Repository<Core.Models.Course>().GetEntityAsync(request.CourseId);
            if (course == null)
                return new ApiResponse(404, "الدورة غير موجودة");

            var spec = new EnrollmentSpecification(request.CourseId, request.UserId);
            var enrollment = await _unitOfWork.Repository<Enrollment>()
                .GetEntityWithSpecAsync(spec);

            if (enrollment == null)
                return new ApiResponse(400, "يجب التسجيل في الدورة أولاً قبل إضافة مراجعة");

            var existingReview = await _unitOfWork.Repository<Core.Models.Review>()
                .GetEntityWithSpecNoTrackingAsync(
                    new ReviewSpecification(request.UserId, request.CourseId));

            if (existingReview != null)
                return new ApiResponse(400, "لقد أضفت مراجعة لهذه الدورة من قبل");

            var reviewText = request.ReviewText?.Trim();
            if (string.IsNullOrWhiteSpace(reviewText) || reviewText.Length < 10 || reviewText.Length > 1000)
                return new ApiResponse(400, "نص المراجعة يجب أن يكون بين 10 أحرف وألف حرف");

            var review = new Core.Models.Review
            {
                Text = reviewText,
                CourseId = request.CourseId,
                AppUserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.Repository<Core.Models.Review>().AddAsync(review);
            await _unitOfWork.Complete();

            return new ApiResponse(200, "تم إضافة المراجعة بنجاح");
        }
    }
}
