using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Quiz
{
    public class QuizWithQuestionsAndChoicesSpecification : BaseSpecification<Core.Models.Quiz>
    {
        public QuizWithQuestionsAndChoicesSpecification(int quizId)
            : base(x => x.Id == quizId)
        {
            Includes.Add(q => q.Questions);
            AddThenInclude(query => query
                .Include(q => q.Questions)
                .ThenInclude(q => q.Choices));
        }
    }
}
