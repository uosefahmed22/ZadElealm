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
        private readonly IMemoryCache _cache;

        public AddRatingCommandHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(AddRatingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingRating = await _unitOfWork.Repository<Core.Models.Rating>()
                    .GetEntityWithSpecAsync(new RatingSpecification(request.UserId, request.CourseId));

                if (existingRating != null)
                    return new ApiResponse(400, "لقد قمت بتقييم هذه الدورة من قبل");

                var course = await _unitOfWork.Repository<Core.Models.Course>().GetByIdAsync(request.CourseId);
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
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Core.Models.Rating>().AddAsync(rating);
                await _unitOfWork.Complete();

                _cache.Remove($"course_ratings_{request.CourseId}");

                await UpdateCourseAverageRating(request.CourseId);

                return new ApiResponse(200, "تم إضافة التقييم بنجاح");
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء إضافة التقييم");
            }
        }

        private async Task UpdateCourseAverageRating(int courseId)
        {
            var spec = new RatingsByCoursesSpecification(courseId);
            var ratings = await _unitOfWork.Repository<Core.Models.Rating>().GetAllWithSpecAsync(spec);

            if (ratings.Any())
            {
                var averageRating = Math.Min(5, ratings.Select(r => r.Value).Average());
                var course = await _unitOfWork.Repository<Core.Models.Course>().GetByIdAsync(courseId);

                if (course != null)
                {
                    course.rating = averageRating;
                    await _unitOfWork.Complete();
                }
            }
        }
    }
}
