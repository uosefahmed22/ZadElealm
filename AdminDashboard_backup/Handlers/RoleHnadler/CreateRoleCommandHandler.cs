using AdminDashboard.Commands.RoleCommand;
using AdminDashboard.Middlwares;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.RoleHnadler
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ApiResponse>
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly string _primaryAdminEmail;
        private readonly int _maxAdminCount;

        public CreateRoleCommandHandler(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _primaryAdminEmail = configuration["AdminSettings:PrimaryAdminEmail"];
            _maxAdminCount = int.Parse(configuration["AdminSettings:MaxAdminCount"] ?? "10");
        }
        public async Task<ApiResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var confirmedAdmin = await _userManager.FindByEmailAsync(_primaryAdminEmail);
            if (confirmedAdmin != null && !confirmedAdmin.EmailConfirmed)
            {
                return new ApiResponse(400, "المستخدم الرئيسي لم يتم تأكيد بريده الإلكتروني بعد.");
            }

            var rolesCount = await _roleManager.Roles.CountAsync();
            if (rolesCount >= _maxAdminCount)
            {
                return new ApiResponse(400, $"لا يمكن إضافة المزيد من الأدوار. الحد الأقصى هو {_maxAdminCount} أدوار.");
            }

            var roleExists = await _roleManager.RoleExistsAsync(request.Name);
            if (roleExists)
            {
                return new ApiResponse(400, "الدور موجود بالفعل!");
            }

            await _roleManager.CreateAsync(new IdentityRole(request.Name));
            return new ApiResponse(200, "تم إنشاء الدور بنجاح.");
        }
    }
}
