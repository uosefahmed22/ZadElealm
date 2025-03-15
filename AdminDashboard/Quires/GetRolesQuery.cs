using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AdminDashboard.Quires
{
    public class GetRolesQuery : IRequest<List<IdentityRole>>
    {
    }
}
