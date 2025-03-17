using AdminDashboard.Quires.QuizQuery;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.QuizHandler
{
    public class GetCoursesForQuizQueryHandler : IRequestHandler<GetCoursesForQuizQuery, IReadOnlyList<Course>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCoursesForQuizQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<Course>> Handle(GetCoursesForQuizQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Repository<Course>().GetAllAsync();
        }
    }

}
