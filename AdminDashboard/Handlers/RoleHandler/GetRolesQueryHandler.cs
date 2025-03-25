using AdminDashboard.Quires.AdminQuery;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Handlers.RoleHandler
{
    public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<IdentityRole>>
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public GetRolesQueryHandler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<List<IdentityRole>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            return await _roleManager.Roles.ToListAsync(cancellationToken);
        }
    }
}
