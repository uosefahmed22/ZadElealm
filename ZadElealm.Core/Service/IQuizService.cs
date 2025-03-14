using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Service.Errors;

namespace ZadElealm.Core.Service
{
    public interface IQuizService
    {
        Task<ServiceApiResponse<int>> CreateQuizAsync(QuizDto quizDto);
        Task<ServiceApiResponse<QuizResultDto>> SubmitQuizAsync(string userId, QuizSubmissionDto submission);
        Task<ServiceApiResponse<bool>> UpdateQuizAsync(QuizDto quizDto);
    }
}
