using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class ProgressWithCompletedQuizSpecification : BaseSpecification<Progress>
    {
        public ProgressWithCompletedQuizSpecification(string userId, int quizId)
            : base(p => p.AppUserId == userId && p.QuizId == quizId && p.IsCompleted)
        {
            Includes.Add(p => p.Quiz);
        }
    }
}
