using ZadElealm.Apis.Commands.VideoProgressCommands;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Videos;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class UpdateVideoProgressHandler : BaseCommandHandler<UpdateVideoProgressCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private const double COMPLETION_THRESHOLD = 0.85;

        public UpdateVideoProgressHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiDataResponse> Handle(UpdateVideoProgressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var videoSpec = new VideoByIdSpecification(request.VideoId);
                var video = await _unitOfWork.Repository<Video>().GetEntityWithSpecAsync(videoSpec);

                if (video == null)
                    return new ApiDataResponse(404, "Video not found");

                var enrollmentSpec = new EnrollmentSpecification(video.CourseId, request.UserId);
                var enrollment = await _unitOfWork.Repository<Enrollment>().GetEntityWithSpecAsync(enrollmentSpec);

                if (enrollment == null)
                    return new ApiDataResponse(403, "You are not enrolled in this course");

                var previousVideosSpec = new VideoProgressWithSpec(request.UserId, video.CourseId);
                var previousVideosProgress = await _unitOfWork.Repository<VideoProgress>()
                    .GetAllWithSpecAsync(previousVideosSpec);

                var canAccessVideo = await CanAccessVideo(video, previousVideosProgress);
                if (!canAccessVideo)
                    return new ApiDataResponse(403, "You need to complete previous videos first");

                var progressSpec = new VideoProgressSpecification(request.UserId, request.VideoId);
                var progress = await _unitOfWork.Repository<VideoProgress>().GetEntityWithSpecAsync(progressSpec);

                if (progress == null)
                {
                    progress = new VideoProgress
                    {
                        UserId = request.UserId,
                        VideoId = request.VideoId,
                        CourseId = video.CourseId,
                        WatchedDuration = request.WatchedDuration,
                        IsCompleted = false
                    };
                    await _unitOfWork.Repository<VideoProgress>().AddAsync(progress);
                }
                else
                {
                    progress.WatchedDuration = request.WatchedDuration;
                    _unitOfWork.Repository<VideoProgress>().Update(progress);
                }

                progress.IsCompleted = (request.WatchedDuration.TotalSeconds / video.VideoDuration.TotalSeconds) >= COMPLETION_THRESHOLD;

                await _unitOfWork.Complete();

                var videoDto = new VideoProgressDto
                {
                    VideoId = progress.VideoId,
                    CourseId = progress.CourseId, 
                    WatchedDuration = progress.WatchedDuration.TotalSeconds,
                    IsCompleted = progress.IsCompleted
                };

                return new ApiDataResponse(200, videoDto);
            }
            catch (Exception ex)
            {
                return new ApiDataResponse(500, "An error occurred while updating video progress");
            }
        }

        private async Task<bool> CanAccessVideo(Video video, IEnumerable<VideoProgress> previousVideosProgress)
        {
            if (video.OrderInCourse == 1)
                return true;

            var previousVideos = previousVideosProgress
                .Where(p => p.Video.OrderInCourse < video.OrderInCourse)
                .OrderBy(p => p.Video.OrderInCourse);

            return previousVideos.All(p => p.IsCompleted);
        }
    }
}
