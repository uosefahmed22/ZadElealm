using Microsoft.EntityFrameworkCore;
namespace ZadElealm.Core.Specifications.Quiz
{
    public class QuizWithQuestionsAndChoicesSpecification : BaseSpecification<Core.Models.Quiz>
    {
        public QuizWithQuestionsAndChoicesSpecification(int quizId)
            : base(x => x.Id == quizId)
        {
            AddThenInclude(query => query
                .Include(q => q.Questions)
                .ThenInclude(q => q.Choices));

            ApplySplitQuery();
        }
    }
}
