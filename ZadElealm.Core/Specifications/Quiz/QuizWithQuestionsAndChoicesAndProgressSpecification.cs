using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Quiz
{
    public class QuizWithQuestionsAndChoicesAndProgressSpecification : BaseSpecification<Core.Models.Quiz>
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
