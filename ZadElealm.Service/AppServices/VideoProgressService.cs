using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Policies;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Course;
using ZadElealm.Core.Specifications.Videos;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Service.AppServices
{
    public class VideoProgressService : IVideoProgressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVideoProgressReadRepository _videoProgressReadRepository;
        private const double CompletionThreshold = 0.85;

        public VideoProgressService(
            IUnitOfWork unitOfWork,
            IVideoProgressReadRepository videoProgressReadRepository)
        {
            _unitOfWork = unitOfWork;
            _videoProgressReadRepository = videoProgressReadRepository;
        }

        public async Task<ApiDataResponse> UpdateProgressAsync(string userId, int videoId, TimeSpan watchedDuration)
            => await UpdateProgressAsync(userId, videoId, watchedDuration, CancellationToken.None);

        public async Task<ApiDataResponse> UpdateProgressAsync(
            string userId,
            int videoId,
            TimeSpan watchedDuration,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId) || videoId <= 0)
                return new ApiDataResponse(400, null, "بيانات المستخدم أو الفيديو غير صالحة");

            if (watchedDuration.TotalSeconds < 0)
                return new ApiDataResponse(400, null, "مدة المشاهدة لا يمكن أن تكون سلبية");

            var video = await _unitOfWork.Repository<Video>()
                .GetEntityWithNoTrackingAsync(videoId, cancellationToken);

            if (video == null)
                return new ApiDataResponse(404, null, "الفيديو غير موجود");

            if (video.CourseId <= 0 || video.VideoDuration <= TimeSpan.Zero)
                return new ApiDataResponse(400, null, "معرف الدورة المرتبط بالفيديو غير صالح");

            if (watchedDuration.TotalSeconds > video.VideoDuration.TotalSeconds)
            {
                return new ApiDataResponse(400, null, $"مدة المشاهدة ({watchedDuration.TotalSeconds} ثانية) لا يمكن أن تتجاوز مدة الفيديو ({video.VideoDuration.TotalSeconds} ثانية)");
            }

            var progressSpec = new VideoProgressSpecification(userId, videoId);
            var progress = await _unitOfWork.Repository<VideoProgress>()
                .GetEntityWithSpecAsync(progressSpec, cancellationToken);

            var completionPercentage = watchedDuration.TotalSeconds / video.VideoDuration.TotalSeconds;
            var isCompleted = completionPercentage >= CompletionThreshold;

            if (progress != null)
            {
                if (progress.WatchedDuration.TotalSeconds > watchedDuration.TotalSeconds)
                    return new ApiDataResponse(200, progress, "التقدم المسجل سابقًا أكبر");

                progress.WatchedDuration = watchedDuration;
                progress.IsCompleted = isCompleted;
                _unitOfWork.Repository<VideoProgress>().Update(progress);
            }
            else
            {
                progress = new VideoProgress
                {
                    UserId = userId,
                    VideoId = videoId,
                    CourseId = video.CourseId,
                    WatchedDuration = watchedDuration,
                    IsCompleted = isCompleted,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<VideoProgress>().AddAsync(progress, cancellationToken);
            }

            await _unitOfWork.Complete(cancellationToken);
            return new ApiDataResponse(200, progress, "تم تحديث التقدم بنجاح");
        }
        public async Task<ApiDataResponse> GetCourseProgressAsync(string userId, int courseId)
            => await GetCourseProgressAsync(userId, courseId, CancellationToken.None);

        public async Task<ApiDataResponse> GetCourseProgressAsync(
            string userId,
            int courseId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId) || courseId <= 0)
                return new ApiDataResponse(400, null, "بيانات المستخدم أو الدورة غير صالحة");

            var summary = await _videoProgressReadRepository
                .GetCourseSummaryAsync(userId, courseId, cancellationToken);
            if (summary == null)
                return new ApiDataResponse(404, null, "الدورة غير موجودة");

            if (!summary.IsEnrolled)
                return new ApiDataResponse(404, null, "المستخدم غير مسجل في هذه الدورة");

            var completedVideos = summary.CompletedVideos;
            var totalVideos = summary.TotalVideos;
            completedVideos = Math.Min(completedVideos, totalVideos);

            var videoProgress = totalVideos > 0
                ? ((float)completedVideos / totalVideos) * 100
                : 0;

            var overallProgress = videoProgress;
            var isEligibleForQuiz = CourseCompletionPolicy.IsEligibleForAssessment(videoProgress);

            var courseProgress = new CourseProgress
            {
                VideoProgress = videoProgress,
                OverallProgress = overallProgress,
                CompletedVideos = completedVideos,
                TotalVideos = totalVideos,
                IsEligibleForQuiz = isEligibleForQuiz
            };
            return new ApiDataResponse(200, courseProgress, "تم استرجاع تقدم الدورة بنجاح");
        }
        public async Task<bool> CheckCourseCompletionEligibilityAsync(string userId, int courseId)
            => await CheckCourseCompletionEligibilityAsync(
                userId,
                courseId,
                CancellationToken.None);

        public async Task<bool> CheckCourseCompletionEligibilityAsync(
            string userId,
            int courseId,
            CancellationToken cancellationToken)
        {
            var progressResponse = await GetCourseProgressAsync(userId, courseId, cancellationToken);

            if (progressResponse.StatusCode != 200 || progressResponse.Data == null)
                return false;

            var courseProgress = progressResponse.Data as CourseProgress;
            return courseProgress != null &&
                CourseCompletionPolicy.IsEligibleForAssessment(courseProgress.OverallProgress);
        }
        public async Task<ApiDataResponse> GetVideoProgressAsync(string userId, int videoId)
            => await GetVideoProgressAsync(userId, videoId, CancellationToken.None);

        public async Task<ApiDataResponse> GetVideoProgressAsync(
            string userId,
            int videoId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId) || videoId <= 0)
                return new ApiDataResponse(400, null, "بيانات المستخدم أو الفيديو غير صالحة");

            var spec = new VideoProgressSpecification(userId, videoId);
            var progress = await _unitOfWork.Repository<VideoProgress>()
                .GetEntityWithSpecNoTrackingAsync(spec, cancellationToken);

            return progress == null
                ? new ApiDataResponse(404, null, "لم يتم العثور على تقدم للفيديو")
                : new ApiDataResponse(200, progress, "تم استرجاع تقدم الفيديو بنجاح");
        }
    }
}
