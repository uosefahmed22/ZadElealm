using MediatR;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Commands.QuizCommands
{
    public record CreateQuizCommand : IRequest<ApiResponse>
    {
        public CreateQuizDto QuizDto { get; init; }

        public CreateQuizCommand(CreateQuizDto quizDto)
        {
            QuizDto = quizDto;
        }
    }
}
