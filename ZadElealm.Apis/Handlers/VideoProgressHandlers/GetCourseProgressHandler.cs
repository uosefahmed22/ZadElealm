using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Quaries.VideoProgressQueries;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class GetCourseProgressHandler : BaseQueryHandler<GetCourseProgressQuery, ApiDataResponse>
    {
        private readonly IVideoProgressService _videoProgressService;

        public GetCourseProgressHandler(IVideoProgressService videoProgressService)
        {
            _videoProgressService = videoProgressService;
        }

        public override async Task<ApiDataResponse> Handle(
            GetCourseProgressQuery request,
            CancellationToken cancellationToken)
        {
            var response = await _videoProgressService
                .GetCourseProgressAsync(request.UserId, request.CourseId, cancellationToken);
            if (response.StatusCode != 200 || response.Data is not CourseProgress progress)
                return response;

            var dto = new CourseProgressDto
            {
                CourseId = request.CourseId,
                VideoProgress = progress.VideoProgress,
                OverallProgress = progress.OverallProgress,
                CompletedVideos = progress.CompletedVideos,
                TotalVideos = progress.TotalVideos,
                IsEligibleForQuiz = progress.IsEligibleForQuiz,
                RemainingVideos = progress.TotalVideos - progress.CompletedVideos
            };

            return new ApiDataResponse(200, dto, response.Message);
        }
    }
}
