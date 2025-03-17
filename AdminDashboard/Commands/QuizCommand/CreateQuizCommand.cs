using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Core.Specifications.Quiz;

namespace AdminDashboard.Commands.QuizCommand
{
    // Commands/CreateQuizCommand.cs
    public class CreateQuizCommand : IRequest<bool>
    {
        public QuizDto QuizDto { get; set; }
    }

    
    
}
