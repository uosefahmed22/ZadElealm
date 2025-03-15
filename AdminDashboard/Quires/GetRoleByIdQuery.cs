using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AdminDashboard.Quires
{
    public class GetRoleByIdQuery : IRequest<IdentityRole>
    {
        public string Id { get; set; }
    }
}
