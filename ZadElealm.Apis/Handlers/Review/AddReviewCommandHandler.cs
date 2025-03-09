using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Review;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Review;

namespace ZadElealm.Apis.Handlers.Review
{
    public class AddReviewCommandHandler : BaseCommandHandler<AddReviewCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public AddReviewCommandHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(AddReviewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingReview = await _unitOfWork.Repository<Core.Models.Review>()
                    .GetEntityWithSpecAsync(new ReviewSpecification(request.UserId, request.CourseId));

                if (existingReview != null)
                    return new ApiResponse(400, "لقد قمت بإضافة مراجعة لهذه الدورة من قبل");

                var course = await _unitOfWork.Repository<Core.Models.Course>().GetByIdAsync(request.CourseId);
                if (course == null)
                    return new ApiResponse(404, "الدورة غير موجودة");

                var spec = new EnrollmentSpecification(request.CourseId, request.UserId);
                var enrollment = await _unitOfWork.Repository<Enrollment>()
                    .GetEntityWithSpecAsync(spec);

                if (enrollment == null)
                    return new ApiResponse(400, "يجب التسجيل في الدورة أولاً قبل إضافة مراجعة");

                if (string.IsNullOrWhiteSpace(request.ReviewText))
                    return new ApiResponse(400, "نص المراجعة مطلوب");

                var review = new Core.Models.Review
                {
                    Text = request.ReviewText.Trim(),
                    courseId = request.CourseId,
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                };

                await _unitOfWork.Repository<Core.Models.Review>().AddAsync(review);
                await _unitOfWork.Complete();

                return new ApiResponse(200, "تم إضافة المراجعة بنجاح");
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء إضافة المراجعة");
            }
        }
    }
}
