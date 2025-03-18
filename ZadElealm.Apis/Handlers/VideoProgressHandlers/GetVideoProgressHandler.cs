using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.VideoProgressQueries;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Videos;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class GetVideoProgressHandler : BaseQueryHandler<GetVideoProgressQuery, VideoProgressDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetVideoProgressHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<VideoProgressDto> Handle(GetVideoProgressQuery request, CancellationToken cancellationToken)
        {
            var spec = new VideoProgressSpecification(request.UserId, request.VideoId);
            var progress = await _unitOfWork.Repository<VideoProgress>().GetEntityWithSpecAsync(spec);

            if (progress == null)
                return null;

            return new VideoProgressDto
            {
                VideoId = progress.VideoId,
                WatchedDuration = progress.WatchedDuration.TotalSeconds,
                IsCompleted = progress.IsCompleted
            };
        }
    }
}
