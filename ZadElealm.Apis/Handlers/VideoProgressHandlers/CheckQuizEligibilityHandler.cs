using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.VideoProgressQueries;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Policies;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.VideoProgressHandlers
{
    public class CheckQuizEligibilityHandler : BaseQueryHandler<CheckQuizEligibilityQuery, ApiDataResponse>
    {
        private readonly IVideoProgressService _videoProgressService;

        public CheckQuizEligibilityHandler(IVideoProgressService videoProgressService)
        {
            _videoProgressService = videoProgressService;
        }

        public override async Task<ApiDataResponse> Handle(
            CheckQuizEligibilityQuery request,
            CancellationToken cancellationToken)
        {
            var response = await _videoProgressService
                .GetCourseProgressAsync(request.UserId, request.CourseId, cancellationToken);
            if (response.StatusCode != 200 || response.Data is not CourseProgress progress)
                return response;

            var isEligible = CourseCompletionPolicy.IsEligibleForAssessment(progress.OverallProgress);
            return new ApiDataResponse(200, new EligibilityResponse
            {
                IsEligible = isEligible,
                Message = isEligible
                    ? "يمكنك الآن الدخول للإختبار"
                    : "الرجاء إكمال جميع دروس الدورة للدخول للاختبار"
            });
        }
    }
}
