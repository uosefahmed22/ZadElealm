using AdminDashboard.Commands.UserCommand;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.UserHandler
{
    public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;

        public UpdateUserRolesCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            user.DisplayName = request.UserName;
            user.IsDeleted = request.IsDeleted;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ApiResponse(400, "Failed to update user");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in request.Roles)
            {
                if (userRoles.Any(r => r == role.Name) && !role.IsSelected)
                    await _userManager.RemoveFromRoleAsync(user, role.Name);

                if (!userRoles.Any(r => r == role.Name) && role.IsSelected)
                    await _userManager.AddToRoleAsync(user, role.Name);
            }

            return new ApiResponse(200, "User updated successfully");
        }
    }
}
