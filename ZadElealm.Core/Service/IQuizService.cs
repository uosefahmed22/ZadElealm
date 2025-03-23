using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Core.Service
{
    public interface IQuizService
    {
        Task<ApiResponse> CreateQuizAsync(CreateQuizDto quizDto);
        Task<ApiDataResponse> SubmitQuizAsync(string userId, QuizSubmissionDto submission);
    }
}
