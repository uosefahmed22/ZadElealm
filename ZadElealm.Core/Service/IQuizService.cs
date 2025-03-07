using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Dtos;

namespace ZadElealm.Core.Service
{
    public interface IQuizService
    {
        Task<QuizResultDto> SubmitQuizAsync(string userId, QuizSubmissionDto submission);
    }
}
