using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.VideoProgressQueries;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class GetVideoProgressHandler : BaseQueryHandler<GetVideoProgressQuery, ApiDataResponse>
    {
        private readonly IVideoProgressService _videoProgressService;

        public GetVideoProgressHandler(IVideoProgressService videoProgressService)
        {
            _videoProgressService = videoProgressService;
        }

        public override async Task<ApiDataResponse> Handle(
            GetVideoProgressQuery request,
            CancellationToken cancellationToken)
        {
            var response = await _videoProgressService
                .GetVideoProgressAsync(request.UserId, request.VideoId, cancellationToken);
            if (response.StatusCode != 200 || response.Data is not VideoProgress progress)
                return response;

            return new ApiDataResponse(200, new VideoProgressDto
            {
                VideoId = progress.VideoId,
                CourseId = progress.CourseId,
                WatchedDuration = progress.WatchedDuration.TotalSeconds,
                IsCompleted = progress.IsCompleted
            }, response.Message);
        }
    }
}
