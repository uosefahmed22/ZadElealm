using AdminDashboard.Commands.UserCommand;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.UserHandler
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;

        public DeleteUserCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return new ApiResponse(404, "User not found");

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return new ApiResponse(400, "Cannot delete an admin user");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return new ApiResponse(500, "An error occurred while deleting the user");
            }

            return new ApiResponse(200, "User deleted successfully");
        }
    }
}
