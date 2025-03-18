using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<VideoProgress> UpdateProgressAsync(string userId, int videoId, TimeSpan watchedDuration)
        {
            var videoSpec = new VideoByIdSpecification(videoId);
            var video = await _unitOfWork.Repository<Video>().GetEntityWithSpecAsync(videoSpec);

            if (video == null)
                throw new Exception($"Video with ID {videoId} not found");

            var progressSpec = new VideoProgressSpecification(userId, videoId);
            var progress = await _unitOfWork.Repository<VideoProgress>().GetEntityWithSpecAsync(progressSpec);

            if (progress == null)
            {
                progress = new VideoProgress
                {
                    UserId = userId,
                    VideoId = videoId,
                    WatchedDuration = watchedDuration,
                    IsCompleted = false
                };

                await _unitOfWork.Repository<VideoProgress>().AddAsync(progress);
            }
            else
            {
                progress.WatchedDuration = watchedDuration;
                _unitOfWork.Repository<VideoProgress>().Update(progress);
            }

            progress.IsCompleted = (watchedDuration.TotalSeconds / video.VideoDuration.TotalSeconds) >= COMPLETION_THRESHOLD;

            await _unitOfWork.Complete();
            return progress;
        }
        public async Task<CourseProgress> GetCourseProgressAsync(string userId, int courseId)
        {
            var spec = new CourseWithVideosAndQuizzesSpecification(courseId);
            var course = await _unitOfWork.Repository<Course>().GetEntityWithSpecAsync(spec);

            if (course == null)
                throw new Exception("Course not found");

            var videoProgressSpec = new VideoProgressWithSpec(userId, courseId);
            var videoProgresses = await _unitOfWork.Repository<VideoProgress>().GetAllWithSpecNoTrackingAsync(videoProgressSpec);

            var completedVideos = videoProgresses.Count(p => p.IsCompleted);
            var totalVideos = course.Videos.Count;

            var videoProgress = totalVideos > 0
                ? ((float)completedVideos / totalVideos) * 100
                : 0;

            var overallProgress = videoProgress;
            var isEligibleForQuiz = videoProgress >= 80;

            return new CourseProgress
            {
                VideoProgress = videoProgress,
                OverallProgress = overallProgress,
                CompletedVideos = completedVideos,
                TotalVideos = totalVideos,
                IsEligibleForQuiz = isEligibleForQuiz
            };
        }
        public async Task<bool> CheckCourseCompletionEligibilityAsync(string userId, int courseId)
        {
            var progress = await GetCourseProgressAsync(userId, courseId);
            return progress.OverallProgress >= 80;
        }
        public async Task<VideoProgress> GetVideoProgressAsync(string userId, int videoId)
        {
            var spec = new VideoProgressWithSpec(userId, videoId);
            return await _unitOfWork.Repository<VideoProgress>().GetEntityWithSpecAsync(spec);
        }
    }
}
