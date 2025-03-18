using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Service
{
    public interface IVideoProgressService
    {
        Task<VideoProgress> UpdateProgressAsync(string userId, int videoId, TimeSpan watchedDuration);
        Task<CourseProgress> GetCourseProgressAsync(string userId, int courseId);
        Task<bool> CheckCourseCompletionEligibilityAsync(string userId, int courseId);
        Task<VideoProgress> GetVideoProgressAsync(string userId, int videoId);
    }
}
