using AdminDashboard.Commands;
using AdminDashboard.Controllers.AdminDashboard.Controllers;
using AdminDashboard.Dto;
using AdminDashboard.Helpers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers
{
    public class AddAdminCommandHandler : IRequestHandler<AddAdminCommand, AddAdminResult>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOptions<AdminSettings> _adminSettings;
        private readonly ILogger<AddAdminCommandHandler> _logger;

        public AddAdminCommandHandler(
            UserManager<AppUser> userManager,
            IOptions<AdminSettings> adminSettings,
            ILogger<AddAdminCommandHandler> logger)
        {
            _userManager = userManager;
            _adminSettings = adminSettings;
            _logger = logger;
        }

        public async Task<AddAdminResult> Handle(AddAdminCommand request, CancellationToken cancellationToken)
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            if (adminUsers.Count >= _adminSettings.Value.MaxAdminCount)
            {
                return new AddAdminResult
                {
                    Succeeded = false,
                    ErrorMessage = $"Cannot add more than {_adminSettings.Value.MaxAdminCount} administrators."
                };
            }

            var user = new AppUser
            {
                DisplayName = request.DisplayName,
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return new AddAdminResult { Succeeded = true };
            }

            return new AddAdminResult
            {
                Succeeded = false,
                Errors = result.Errors
            };
        }
    }
}
