using MediatR;
using ZadElealm.Apis.Commands.QuizCommands;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications.Quiz;

namespace ZadElealm.Apis.Handlers.QuizHandler
{
    public class CreateQuizCommandHandler : IRequestHandler<CreateQuizCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQuizService _quizService;
        public CreateQuizCommandHandler(IUnitOfWork unitOfWork, IQuizService quizService)
        {
            _unitOfWork = unitOfWork;
            _quizService = quizService;
        }

        public async Task<ApiResponse> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
        {
            var spec = new QuizWithCourseSpecification(request.QuizDto.CourseId);
            var quiz = await _unitOfWork.Repository<Core.Models.Quiz>().GetEntityWithSpecNoTrackingAsync(spec);
            if (quiz != null)
            {
                return new ApiResponse(400, "الدورة لديها اختبار مسبق");
            }

            var response = await _quizService.CreateQuizAsync(request.QuizDto);
            
            return response;
        }
    }
}