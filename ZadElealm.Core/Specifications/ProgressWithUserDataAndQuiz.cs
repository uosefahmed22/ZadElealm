using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications
{
    public class ProgressWithUserDataAndQuiz : BaseSpecification<Core.Models.Progress>
    {
        public ProgressWithUserDataAndQuiz(string userId, int quizId)
            : base(c => c.AppUserId == userId && c.QuizId == quizId)
        {
            Includes.Add(c => c.Quiz);
            Includes.Add(c => c.AppUser);
        }
    }
}