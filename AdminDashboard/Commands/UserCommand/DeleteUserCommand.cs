using MediatR;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.UserCommand
{
    public class DeleteUserCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }
}
