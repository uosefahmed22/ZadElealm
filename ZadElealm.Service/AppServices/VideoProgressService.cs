using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Errors;
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
        private const double COMPLETION_THRESHOLD = 0.85;

        public VideoProgressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiDataResponse> UpdateProgressAsync(string userId, int videoId, TimeSpan watchedDuration)
        {
            if (watchedDuration.TotalSeconds < 0)
                return new ApiDataResponse(400, null, "Watch duration cannot be negative");

            var videoSpec = new VideoByIdSpecification(videoId);
            var video = await _unitOfWork.Repository<Video>().GetEntityWithSpecAsync(videoSpec);

            if (video == null)
                return new ApiDataResponse(404, null, "Video not found");

            if (video.CourseId <= 0)
                return new ApiDataResponse(400, null, "Invalid CourseId associated with the video");

            if (watchedDuration.TotalSeconds > video.VideoDuration.TotalSeconds)
            {
                return new ApiDataResponse(400, null, $"Watch duration ({watchedDuration.TotalSeconds} seconds) cannot exceed video duration ({video.VideoDuration.TotalSeconds} seconds)");
            }

            var progressSpec = new VideoProgressSpecification(userId, videoId);
            var progress = await _unitOfWork.Repository<VideoProgress>().GetEntityWithSpecAsync(progressSpec);

            double completionPercentage = watchedDuration.TotalSeconds / video.VideoDuration.TotalSeconds;
            bool isCompleted = completionPercentage >= COMPLETION_THRESHOLD;

            if (progress != null)
            {
                if (progress.WatchedDuration.TotalSeconds > watchedDuration.TotalSeconds)
                    return new ApiDataResponse(200, progress, "Previously recorded progress is greater");

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

            try
            {
                await _unitOfWork.Complete();
                return new ApiDataResponse(200, progress, "Progress updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiDataResponse(500, null, $"Failed to update progress: {ex.Message}");
            }
        }
        public async Task<ApiDataResponse> GetCourseProgressAsync(string userId, int courseId)
        {
            var spec = new CourseWithVideosAndQuizzesSpecification(courseId);
            var course = await _unitOfWork.Repository<Course>().GetEntityWithSpecAsync(spec);

            if (course == null)
                return new ApiDataResponse(404, null, "Course not found");

            var enrollmentSpec = new EnrollmentSpecification(courseId, userId);
            var enrollment = await _unitOfWork.Repository<Enrollment>().GetEntityWithSpecAsync(enrollmentSpec);

            if (enrollment == null)
                return new ApiDataResponse(404, null, "User is not enrolled in this course");

            var videoProgressSpec = new VideoProgressWithSpec(userId, courseId);
            var videoProgresses = await _unitOfWork.Repository<VideoProgress>().GetAllWithSpecNoTrackingAsync(videoProgressSpec);

            var completedVideos = videoProgresses.Count(p => p.IsCompleted);
            var totalVideos = course.Videos.Count;

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
            return new ApiDataResponse(200, courseProgress, "Course progress retrieved successfully");
        }
        public async Task<bool> CheckCourseCompletionEligibilityAsync(string userId, int courseId)
        {
            var progress = await GetCourseProgressAsync(userId, courseId);
            var courseProgress = progress.Data as CourseProgress;
            return courseProgress.OverallProgress >= 80;
        }
        public async Task<ApiDataResponse> GetVideoProgressAsync(string userId, int videoId)
        {
            var spec = new VideoProgressWithSpec(userId, videoId);
            await _unitOfWork.Repository<VideoProgress>().GetEntityWithSpecAsync(spec);
            return new ApiDataResponse(200, spec, "Video progress retrieved successfully");
        }
    }
}
