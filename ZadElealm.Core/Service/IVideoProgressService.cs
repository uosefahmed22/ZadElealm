using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Service
{
    public interface IVideoProgressService
    {
        Task<ApiDataResponse> UpdateProgressAsync(string userId, int videoId, TimeSpan watchedDuration);
        Task<ApiDataResponse> UpdateProgressAsync(string userId, int videoId, TimeSpan watchedDuration, CancellationToken cancellationToken);
        Task<ApiDataResponse> GetCourseProgressAsync(string userId, int courseId);
        Task<ApiDataResponse> GetCourseProgressAsync(string userId, int courseId, CancellationToken cancellationToken);
        Task<bool> CheckCourseCompletionEligibilityAsync(string userId, int courseId);
        Task<bool> CheckCourseCompletionEligibilityAsync(string userId, int courseId, CancellationToken cancellationToken);
        Task<ApiDataResponse> GetVideoProgressAsync(string userId, int videoId);
        Task<ApiDataResponse> GetVideoProgressAsync(string userId, int videoId, CancellationToken cancellationToken);
    }
}
