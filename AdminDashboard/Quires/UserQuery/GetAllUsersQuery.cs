using AdminDashboard.Models;
using MediatR;

namespace AdminDashboard.Quires.UserQuery
{
    public class GetAllUsersQuery : IRequest<IEnumerable<UserViewModel>>
    {
    }
}
