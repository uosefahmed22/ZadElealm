using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Quires.QuizQuery
{
    public class GetCoursesForQuizQuery : IRequest<IReadOnlyList<Course>>
    {
    }
}
