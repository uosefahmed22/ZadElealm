using AdminDashboard.Quires;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AdminDashboard.Handlers.AuthHandler
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, IdentityRole>
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public GetRoleByIdQueryHandler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IdentityRole> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _roleManager.FindByIdAsync(request.Id);
        }
    }
}
