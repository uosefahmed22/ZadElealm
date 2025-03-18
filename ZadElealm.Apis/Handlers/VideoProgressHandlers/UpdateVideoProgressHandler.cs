using ZadElealm.Apis.Commands.VideoProgressCommands;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Videos;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class UpdateVideoProgressHandler : BaseCommandHandler<UpdateVideoProgressCommand, VideoProgressDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private const double COMPLETION_THRESHOLD = 0.85;

        public UpdateVideoProgressHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<VideoProgressDto> Handle(UpdateVideoProgressCommand request, CancellationToken cancellationToken)
        {
            var videoSpec = new VideoByIdSpecification(request.VideoId);
            var video = await _unitOfWork.Repository<Video>().GetEntityWithSpecAsync(videoSpec);

            if (video == null)
                throw new Exception($"Video with ID {request.VideoId} not found");

            var progressSpec = new VideoProgressSpecification(request.UserId, request.VideoId);
            var progress = await _unitOfWork.Repository<VideoProgress>().GetEntityWithSpecAsync(progressSpec);

            if (progress == null)
            {
                progress = new VideoProgress
                {
                    UserId = request.UserId,
                    VideoId = request.VideoId,
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

            return new VideoProgressDto
            {
                VideoId = progress.VideoId,
                WatchedDuration = progress.WatchedDuration.TotalSeconds,
                IsCompleted = progress.IsCompleted
            };
        }
    }
}
