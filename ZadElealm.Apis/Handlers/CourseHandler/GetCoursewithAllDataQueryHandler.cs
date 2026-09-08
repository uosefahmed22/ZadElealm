using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Course;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Course;
using ZadElealm.Core.Specifications.Videos;

namespace ZadElealm.Apis.Handlers.Course
{
    public class GetCourseWithAllDataQueryHandler : BaseQueryHandler<GetCourseWithAllDataQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnrollmentReadRepository _enrollmentReadRepository;

        public GetCourseWithAllDataQueryHandler(
            IUnitOfWork unitOfWork,
            IEnrollmentReadRepository enrollmentReadRepository)
        {
            _unitOfWork = unitOfWork;
            _enrollmentReadRepository = enrollmentReadRepository;
        }

        public override async Task<ApiResponse> Handle(GetCourseWithAllDataQuery request, CancellationToken cancellationToken)
        {
            var spec = new CourseWithAllDataSpecification(request.CourseId);
            var course = await _unitOfWork.Repository<Core.Models.Course>()
                .GetEntityWithSpecNoTrackingAsync(spec);

            if (course == null)
                return new ApiResponse(404, "الدورة غير موجودة");

            var mappedCourse = course.ToDetailsDto();
            var enrollmentSummary = await _enrollmentReadRepository
                .GetCourseEnrollmentSummaryAsync(
                    request.CourseId,
                    request.UserId,
                    cancellationToken);
            mappedCourse.IsEnrolled = enrollmentSummary.IsCurrentUserEnrolled;
            mappedCourse.TotalEnrolledStudents = enrollmentSummary.TotalEnrolledStudents;

            var sourceReviewsById = course.Review.ToDictionary(review => review.Id);
            foreach (var review in mappedCourse.Review)
            {
                review.IsOwnedByCurrentUser = review.AppUserId == request.UserId;
                var sourceReview = sourceReviewsById[review.Id];
                review.IsLikedByCurrentUser = sourceReview.Likes.Any(like => like.AppUserId == request.UserId);
            }

            if (mappedCourse.IsEnrolled)
            {
                var specvideoProgress = new VideoProgressWithCourseAndUserSpecification(request.UserId, request.CourseId);
                var videoProgress = await _unitOfWork.Repository<VideoProgress>()
                    .GetAllWithSpecNoTrackingAsync(specvideoProgress);

                var progressByVideoId = videoProgress.ToDictionary(progress => progress.VideoId);
                foreach (var video in mappedCourse.Videos)
                {
                    if (progressByVideoId.TryGetValue(video.Id, out var progress))
                    {
                        video.IsCompleted = progress.IsCompleted;
                        video.WatchedDuration = progress.WatchedDuration;
                    }
                }
            }
            else
            {
                foreach (var video in mappedCourse.Videos)
                {
                    video.VideoUrl = string.Empty;
                }
            }

            return new ApiDataResponse(200, mappedCourse);
        }
    }
}
