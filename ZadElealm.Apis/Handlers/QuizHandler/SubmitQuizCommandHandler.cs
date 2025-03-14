using ZadElealm.Apis.Commands.QuizCommands;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.QuizHandler
{
    public class SubmitQuizCommandHandler : BaseCommandHandler<SubmitQuizCommand, ApiResponse>
    {
        private readonly IQuizService _quizService;

        public SubmitQuizCommandHandler(IQuizService quizService)
        {
            _quizService = quizService;
        }

        public override async Task<ApiResponse> Handle(SubmitQuizCommand request, CancellationToken cancellationToken)
        {
            var result = await _quizService.SubmitQuizAsync(request.UserId, request.Submission);

            if (!result.IsSuccess)
                return new ApiResponse(400, result.Message);

            return new ApiDataResponse(200, result.Data);
        }
    }
}
