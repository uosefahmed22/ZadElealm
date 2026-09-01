using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
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
        private const double CompletionThreshold = 0.85;

        public VideoProgressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiDataResponse> UpdateProgressAsync(string userId, int videoId, TimeSpan watchedDuration)
        {
            if (string.IsNullOrWhiteSpace(userId) || videoId <= 0)
                return new ApiDataResponse(400, null, "بيانات المستخدم أو الفيديو غير صالحة");

            if (watchedDuration.TotalSeconds < 0)
                return new ApiDataResponse(400, null, "مدة المشاهدة لا يمكن أن تكون سلبية");

            var video = await _unitOfWork.Repository<Video>().GetEntityWithNoTrackingAsync(videoId);

            if (video == null)
                return new ApiDataResponse(404, null, "الفيديو غير موجود");

            if (video.CourseId <= 0 || video.VideoDuration <= TimeSpan.Zero)
                return new ApiDataResponse(400, null, "معرف الدورة المرتبط بالفيديو غير صالح");

            if (watchedDuration.TotalSeconds > video.VideoDuration.TotalSeconds)
            {
                return new ApiDataResponse(400, null, $"مدة المشاهدة ({watchedDuration.TotalSeconds} ثانية) لا يمكن أن تتجاوز مدة الفيديو ({video.VideoDuration.TotalSeconds} ثانية)");
            }

            var progressSpec = new VideoProgressSpecification(userId, videoId);
            var progress = await _unitOfWork.Repository<VideoProgress>().GetEntityWithSpecAsync(progressSpec);

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

                await _unitOfWork.Repository<VideoProgress>().AddAsync(progress);
            }

            await _unitOfWork.Complete();
            return new ApiDataResponse(200, progress, "تم تحديث التقدم بنجاح");
        }
        public async Task<ApiDataResponse> GetCourseProgressAsync(string userId, int courseId)
        {
            if (string.IsNullOrWhiteSpace(userId) || courseId <= 0)
                return new ApiDataResponse(400, null, "بيانات المستخدم أو الدورة غير صالحة");

            var course = await _unitOfWork.Repository<Course>().GetEntityWithNoTrackingAsync(courseId);

            if (course == null)
                return new ApiDataResponse(404, null, "الدورة غير موجودة");

            var enrollmentSpec = new EnrollmentExistsSpecification(courseId, userId);
            var enrollment = await _unitOfWork.Repository<Enrollment>().GetEntityWithSpecNoTrackingAsync(enrollmentSpec);

            if (enrollment == null)
                return new ApiDataResponse(404, null, "المستخدم غير مسجل في هذه الدورة");

            var completedVideos = await _unitOfWork.Repository<VideoProgress>()
                .CountAsync(new CompletedVideoProgressSpecification(userId, courseId));
            var totalVideos = await _unitOfWork.Repository<Video>()
                .CountAsync(new VideosByCourseSpecification(courseId));
            completedVideos = Math.Min(completedVideos, totalVideos);

            var videoProgress = totalVideos > 0
                ? ((float)completedVideos / totalVideos) * 100
                : 0;

            var overallProgress = videoProgress;
            var isEligibleForQuiz = videoProgress >= 80;

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
        {
            var progressResponse = await GetCourseProgressAsync(userId, courseId);

            if (progressResponse.StatusCode != 200 || progressResponse.Data == null)
                return false;

            var courseProgress = progressResponse.Data as CourseProgress;
            return courseProgress?.OverallProgress >= 80;
        }
        public async Task<ApiDataResponse> GetVideoProgressAsync(string userId, int videoId)
        {
            if (string.IsNullOrWhiteSpace(userId) || videoId <= 0)
                return new ApiDataResponse(400, null, "بيانات المستخدم أو الفيديو غير صالحة");

            var spec = new VideoProgressSpecification(userId, videoId);
            var progress = await _unitOfWork.Repository<VideoProgress>()
                .GetEntityWithSpecNoTrackingAsync(spec);

            return progress == null
                ? new ApiDataResponse(404, null, "لم يتم العثور على تقدم للفيديو")
                : new ApiDataResponse(200, progress, "تم استرجاع تقدم الفيديو بنجاح");
        }
    }
}
