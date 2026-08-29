using MediatR;
using ZadElealm.Core.Errors;

namespace AdminDashboard.Commands.UserCommand
{
    public class DeleteUserCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }
}
