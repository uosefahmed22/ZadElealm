using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AdminDashboard.Quires.AdminQuery
{
    public class GetRolesQuery : IRequest<List<IdentityRole>>
    {
    }
}
