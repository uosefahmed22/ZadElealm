using AdminDashboard.Quires.QuizQuery;
using AutoMapper;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Core.Specifications.Quiz;

namespace AdminDashboard.Handlers.QuizHandler
{
    public class GetAllQuizzesQueryHandler : IRequestHandler<GetAllQuizzesQuery, List<QuizDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllQuizzesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<QuizDto>> Handle(GetAllQuizzesQuery request, CancellationToken cancellationToken)
        {
            var quizzes = await _unitOfWork.Repository<Quiz>()
                .GetAllWithNoTrackingAsync();

            var quizDtos = _mapper.Map<List<QuizDto>>(quizzes);
            return quizDtos;
        }
    }

}
