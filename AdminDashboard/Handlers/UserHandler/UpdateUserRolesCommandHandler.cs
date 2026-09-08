using AdminDashboard.Commands.UserCommand;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Core.Errors;
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
            var user = await _userManager.FindByIdAsync(request.Model.UserId);
            if (user == null)
                return new ApiResponse(404, "User not found");

            user.DisplayName = request.Model.UserName;
            user.IsDeleted = request.Model.IsDeleted;
            user.EmailConfirmed = request.Model.IsConfirmed;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ApiResponse(400, "Failed to update user");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in request.Model.Roles)
            {
                if (userRoles.Any(r => r == role.Name) && !role.IsSelected)
                {
                    var removeResult = await _userManager.RemoveFromRoleAsync(user, role.Name);
                    if (!removeResult.Succeeded)
                        return new ApiResponse(400, "Failed to update user roles");
                }
                if (!userRoles.Any(r => r == role.Name) && role.IsSelected)
                {
                    var addResult = await _userManager.AddToRoleAsync(user, role.Name);
                    if (!addResult.Succeeded)
                        return new ApiResponse(400, "Failed to update user roles");
                }
            }

            return new ApiResponse(200);
        }
    }
}
