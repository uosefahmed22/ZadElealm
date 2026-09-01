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

        public GetCourseWithAllDataQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(GetCourseWithAllDataQuery request, CancellationToken cancellationToken)
        {
            var spec = new CourseWithAllDataSpecification(request.CourseId);
            var course = await _unitOfWork.Repository<Core.Models.Course>()
                .GetEntityWithSpecNoTrackingAsync(spec);

            if (course == null)
                return new ApiResponse(404, "الدورة غير موجودة");

            var mappedCourse = course.ToDetailsDto();
            var enrollment = await _unitOfWork.Repository<Enrollment>()
                .GetEntityWithSpecNoTrackingAsync(
                    new EnrollmentExistsSpecification(request.CourseId, request.UserId));
            mappedCourse.IsEnrolled = enrollment != null;
            foreach (var review in mappedCourse.Review)
            {
                review.IsOwnedByCurrentUser = review.AppUserId == request.UserId;
            }
            foreach (var review in mappedCourse.Review)
            {
                var sourceReview = course.Review.First(source => source.Id == review.Id);
                review.IsLikedByCurrentUser = sourceReview.Likes.Any(like => like.AppUserId == request.UserId);
            }

            if (mappedCourse.IsEnrolled)
            {
                var specvideoProgress = new VideoProgressWithCourseAndUserSpecification(request.UserId, request.CourseId);
                var videoProgress = await _unitOfWork.Repository<VideoProgress>()
                    .GetAllWithSpecNoTrackingAsync(specvideoProgress);

                foreach (var video in mappedCourse.Videos)
                {
                    var progress = videoProgress.FirstOrDefault(vp => vp.VideoId == video.Id);
                    if (progress != null)
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
