using AdminDashboard.Models;
using MediatR;

namespace AdminDashboard.Quires.UserQuery
{
    public class GetUserForEditQuery : IRequest<UserRolesViewModel>
    {
        public string UserId { get; set; }
    }
}
