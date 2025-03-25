using AdminDashboard.Commands;
using AdminDashboard.Middlwares;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Handlers.AuthHandler
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, ApiResponse>
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public UpdateRoleCommandHandler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<ApiResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var roleExists = await _roleManager.RoleExistsAsync(request.Name);
            if (roleExists)
                return new ApiResponse(400, "الدور موجود بالفعل!");

            var roleToUpdate = await _roleManager.FindByIdAsync(request.Id);
            if (roleToUpdate == null)
                return new ApiResponse(400, "الدور غير موجود!");

            roleToUpdate.Name = request.Name;
            var ApiResponse = await _roleManager.UpdateAsync(roleToUpdate);

            return ApiResponse.Succeeded
                ? new ApiResponse(400, "تم تحديث الدور بنجاح.")
                : new ApiResponse(400, "حدث خطأ أثناء تحديث الدور.");
        }
    }
}
