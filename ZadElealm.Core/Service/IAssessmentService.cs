using ZadElealm.Core.Errors;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Core.Service;

public interface IAssessmentService
{
    Task<ApiDataResponse> GetAvailableAssessmentsAsync(string userId);
    Task<ApiDataResponse> GetAssessmentAsync(string userId, int assessmentId);
    Task<ApiDataResponse> SubmitAssessmentAsync(
        string userId,
        int assessmentId,
        AssessmentSubmissionDto submission);
}
