using AdminDashboard.Commands.RoleCommand;
using AdminDashboard.Middlwares;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Handlers.RoleHnadler
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, ApiResponse>
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public DeleteRoleCommandHandler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<ApiResponse> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.Id);

            if (role == null)
                return new ApiResponse(404,"الدور غير موجود!");

            if (role.Name == "Admin" || role.Name == "User")
                return new ApiResponse(400,"لا يمكن حذف هذا الدور!");

            var ApiResponse = await _roleManager.DeleteAsync(role);
            return ApiResponse.Succeeded
                ? new ApiResponse(400, "تم حذف الدور بنجاح.")
                : new ApiResponse(400, "حدث خطأ أثناء حذف الدور.");
        }
    }
}
