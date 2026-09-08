using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Rating;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Rating;

namespace ZadElealm.Apis.Handlers.Rating
{
    public class AddRatingCommandHandler : BaseCommandHandler<AddRatingCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRatingReadRepository _ratingReadRepository;

        public AddRatingCommandHandler(
            IUnitOfWork unitOfWork,
            IRatingReadRepository ratingReadRepository)
        {
            _unitOfWork = unitOfWork;
            _ratingReadRepository = ratingReadRepository;
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

            var ratingSummary = await _ratingReadRepository
                .GetSummaryAsync(request.CourseId, cancellationToken);
            course.rating = CalculateAverage(ratingSummary, request.Value);

            await _unitOfWork.Repository<Core.Models.Rating>().AddAsync(rating);
            await _unitOfWork.Complete();

            return new ApiResponse(200, "تم إضافة التقييم بنجاح");
        }

        private static decimal CalculateAverage(
            RatingSummaryReadModel current,
            decimal newRating)
            => Math.Min(
                5m,
                ((((decimal)current.Average) * current.Count) + newRating) /
                    (current.Count + 1));
    }
}
