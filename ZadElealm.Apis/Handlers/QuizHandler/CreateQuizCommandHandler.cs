using MediatR;
using ZadElealm.Apis.Commands.QuizCommands;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.QuizHandler
{
    public class CreateQuizCommandHandler : IRequestHandler<CreateQuizCommand, ApiResponse>
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<CreateQuizCommandHandler> _logger;

        public CreateQuizCommandHandler(IQuizService quizService, ILogger<CreateQuizCommandHandler> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting quiz creation process");
                var response = await _quizService.CreateQuizAsync(request.QuizDto);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateQuizCommandHandler");
                return new ApiResponse(500, "An unexpected error occurred while creating the quiz");
            }
        }
    }
}
