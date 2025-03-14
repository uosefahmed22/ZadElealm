using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class ProgressByQuizAndUserSpecification : BaseSpecification<Progress>
    {
        public ProgressByQuizAndUserSpecification(int quizId, string userId)
            : base(x => x.QuizId == quizId && x.AppUserId == userId)
        {
        }
    }
}
