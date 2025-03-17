using MediatR;
using ZadElealm.Core.ServiceDto;

namespace AdminDashboard.Quires.QuizQuery
{
    public class GetAllQuizzesQuery : IRequest<List<QuizDto>>
    {
    }
}
