using AdminDashboard.Models;
using AdminDashboard.Quires.UserQuery;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.UserHandler
{
    public class GetUserForEditQueryHandler : IRequestHandler<GetUserForEditQuery, UserRolesViewModel>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public GetUserForEditQueryHandler(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<UserRolesViewModel> Handle(GetUserForEditQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            var allRoles = await _roleManager.Roles.ToListAsync(cancellationToken);

            var roles = new List<RoleViewModel>(allRoles.Count);
            foreach (var role in allRoles)
            {
                var roleName = role.Name ?? string.Empty;
                roles.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = roleName,
                    IsSelected = await _userManager.IsInRoleAsync(user, roleName)
                });
            }

            return new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.DisplayName,
                IsDeleted = user.IsDeleted,
                IsConfirmed = user.EmailConfirmed,
                Roles = roles
            };
        }
    }
}
