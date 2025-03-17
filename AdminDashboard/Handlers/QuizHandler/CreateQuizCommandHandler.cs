using AdminDashboard.Commands.QuizCommand;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications.Quiz;

namespace AdminDashboard.Handlers.QuizHandler
{
    public class CreateQuizCommandHandler : IRequestHandler<CreateQuizCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQuizService _quizService;

        public CreateQuizCommandHandler(IUnitOfWork unitOfWork, IQuizService quizService)
        {
            _unitOfWork = unitOfWork;
            _quizService = quizService;
        }

        public async Task<bool> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
        {
            var spec = new QuizWithCourseSpecification(request.QuizDto.CourseId);
            var existingQuiz = await _unitOfWork.Repository<Quiz>()
                .GetEntityWithSpecAsync(spec);

            if (existingQuiz != null)
                return false;

            await _quizService.CreateQuizAsync(request.QuizDto);
            return true;
        }
    }
}
