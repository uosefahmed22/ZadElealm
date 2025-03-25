using AdminDashboard.Controllers.AdminDashboard.Controllers;
using AdminDashboard.Dto;
using AdminDashboard.Helpers;
using AdminDashboard.Quires;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.AuthHandler
{
    public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsResult>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOptions<AdminSettings> _adminSettings;

        public GetAdminStatsQueryHandler(
            UserManager<AppUser> userManager,
            IOptions<AdminSettings> adminSettings)
        {
            _userManager = userManager;
            _adminSettings = adminSettings;
        }

        public async Task<AdminStatsResult> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            return new AdminStatsResult
            {
                CurrentAdminCount = adminUsers.Count,
                MaxAdminCount = _adminSettings.Value.MaxAdminCount
            };
        }
    }
}
