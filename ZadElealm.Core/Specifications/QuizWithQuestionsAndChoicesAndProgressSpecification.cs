using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class QuizWithQuestionsAndChoicesAndProgressSpecification : BaseSpecification<Quiz>
    {
        public QuizWithQuestionsAndChoicesAndProgressSpecification(int quizId) : base(x => x.Id == quizId)
        {
            Includes.Add(q => q.Progresses);
            Includes.Add(q => q.Questions);
            Includes.Add(Includes => Includes.Course);

            AddThenInclude(query => query
                .Include(q => q.Questions)
                .ThenInclude(question => question.Choices));
        }
    }
}
