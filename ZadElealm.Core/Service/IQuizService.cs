using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models.ServiceDto;

namespace ZadElealm.Core.Service
{
    public interface IQuizService
    {
        Task CreateQuizAsync(QuizDto quizDto);
        Task<QuizResultDto> SubmitQuizAsync(string userId, QuizSubmissionDto submission);
    }
}
