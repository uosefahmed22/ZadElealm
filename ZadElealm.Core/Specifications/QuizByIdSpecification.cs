using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class QuizByIdSpecification : BaseSpecification<Quiz>
    {
        public QuizByIdSpecification(int quizId)
            : base(q => q.Id == quizId)
        {
            Includes.Add(q => q.Course);
            Includes.Add(q => q.Questions);
        }
    }
}
