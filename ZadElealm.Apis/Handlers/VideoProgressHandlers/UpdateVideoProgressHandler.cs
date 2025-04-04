﻿using ZadElealm.Apis.Commands.VideoProgressCommands;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Videos;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class UpdateVideoProgressHandler : BaseCommandHandler<UpdateVideoProgressCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVideoProgressService _videoProgressService;

        public UpdateVideoProgressHandler(
            IUnitOfWork unitOfWork,
            IVideoProgressService videoProgressService)
        {
            _unitOfWork = unitOfWork;
            _videoProgressService = videoProgressService;
        }

        public override async Task<ApiDataResponse> Handle(UpdateVideoProgressCommand request, CancellationToken cancellationToken)
        {
            var videoSpec = new VideoByIdSpecification(request.VideoId);
            var video = await _unitOfWork.Repository<Video>().GetEntityWithSpecAsync(videoSpec);

            if (video == null)
                return new ApiDataResponse(404, "الفيديو غير موجود");

            var enrollmentSpec = new EnrollmentSpecification(video.CourseId, request.UserId);
            var enrollment = await _unitOfWork.Repository<Enrollment>().GetEntityWithSpecAsync(enrollmentSpec);

            if (enrollment == null)
                return new ApiDataResponse(403, "أنت غير مسجل في هذه الدورة");

            var previousVideosSpec = new VideoProgressWithSpec(request.UserId, video.CourseId);
            var previousVideosProgress = await _unitOfWork.Repository<VideoProgress>()
                .GetAllWithSpecAsync(previousVideosSpec);

            var canAccessVideo = await CanAccessVideo(video, previousVideosProgress);
            if (!canAccessVideo)
                return new ApiDataResponse(403, "يجب عليك إكمال الفيديوهات السابقة أولاً");

            var progressResponse = await _videoProgressService.UpdateProgressAsync(
                request.UserId,
                request.VideoId,
                request.WatchedDuration
            );

            if (progressResponse.StatusCode != 200)
                return progressResponse;

            var progress = progressResponse.Data as VideoProgress;
            if (progress == null)
            {
                return new ApiDataResponse(500, "استجابة تقدم غير صالحة");
            }

            var videoDto = new VideoProgressDto
            {
                VideoId = request.VideoId,
                CourseId = video.CourseId,
                WatchedDuration = request.WatchedDuration.TotalSeconds,
                IsCompleted = progress.IsCompleted
            };

            return new ApiDataResponse(200, videoDto);
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
