using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Rating;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Rating;

namespace ZadElealm.Apis.Handlers.Rating
{
    public class AddRatingCommandHandler : BaseCommandHandler<AddRatingCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AddRatingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(AddRatingCommand request, CancellationToken cancellationToken)
        {
            var existingRating = await _unitOfWork.Repository<Core.Models.Rating>()
                .GetEntityWithSpecAsync(new RatingSpecification(request.UserId, request.CourseId));

            if (existingRating != null)
                return new ApiResponse(400, "لقد قمت بتقييم هذه الدورة من قبل");

            var course = await _unitOfWork.Repository<Core.Models.Course>().GetEntityAsync(request.CourseId);
            if (course == null)
                return new ApiResponse(404, "الدورة غير موجودة");

            var enrollment = await _unitOfWork.Repository<Enrollment>()
                .GetEntityWithSpecAsync(new EnrollmentSpecification(request.CourseId, request.UserId));

            if (enrollment == null)
                return new ApiResponse(400, "يجب التسجيل في الدورة أولاً قبل تقييمها");

            if (request.Value < 1 || request.Value > 5)
                return new ApiResponse(400, "قيمة التقييم يجب أن تكون بين 1 و 5");

            var rating = new Core.Models.Rating
            {
                Value = request.Value,
                courseId = request.CourseId,
                AppUserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Core.Models.Rating>().AddAsync(rating);
            await _unitOfWork.Complete();

            await UpdateCourseAverageRating(request.CourseId);

            return new ApiResponse(200, "تم إضافة التقييم بنجاح");
        }

        private async Task UpdateCourseAverageRating(int courseId)
        {
            var spec = new RatingsByCoursesSpecification(courseId);
            var ratings = await _unitOfWork.Repository<Core.Models.Rating>().GetAllWithSpecAsync(spec);

            if (ratings.Any())
            {
                var averageRating = Math.Min(5, ratings.Select(r => r.Value).Average());
                var course = await _unitOfWork.Repository<Core.Models.Course>().GetEntityAsync(courseId);

                if (course != null)
                {
                    course.rating = averageRating;
                    await _unitOfWork.Complete();
                }
            }
        }
    }
}
