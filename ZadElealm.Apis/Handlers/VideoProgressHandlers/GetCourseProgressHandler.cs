using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.VideoProgressQueries;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Course;
using ZadElealm.Core.Specifications.Videos;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class GetCourseProgressHandler : BaseQueryHandler<GetCourseProgressQuery, CourseProgressDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCourseProgressHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<CourseProgressDto> Handle(GetCourseProgressQuery request, CancellationToken cancellationToken)
        {
            var spec = new CourseWithVideosAndQuizzesSpecification(request.CourseId);
            var course = await _unitOfWork.Repository<Core.Models.Course>().GetEntityWithSpecAsync(spec);

            if (course == null)
                throw new Exception($"Course with ID {request.CourseId} not found");

            var videoProgressSpec = new VideoProgressWithSpec(request.UserId, request.CourseId);
            var videoProgresses = await _unitOfWork.Repository<VideoProgress>()
                .GetAllWithSpecNoTrackingAsync(videoProgressSpec);

            var completedVideos = videoProgresses.Count(p => p.IsCompleted);
            var totalVideos = course.Videos.Count;

            var videoProgress = totalVideos > 0
                ? ((float)completedVideos / totalVideos) * 100
                : 0;

            return new CourseProgressDto
            {
                VideoProgress = videoProgress,
                OverallProgress = videoProgress,
                CompletedVideos = completedVideos,
                TotalVideos = totalVideos,
                IsEligibleForQuiz = videoProgress >= 80,
                RemainingVideos = totalVideos - completedVideos
            };
        }
    }

}
