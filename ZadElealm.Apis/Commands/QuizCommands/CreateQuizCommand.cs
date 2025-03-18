using MediatR;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Commands.QuizCommands
{
    public record CreateQuizCommand : IRequest<ApiResponse>
    {
        public QuizDto QuizDto { get; init; }

        public CreateQuizCommand(QuizDto quizDto)
        {
            QuizDto = quizDto;
        }
    }
}
