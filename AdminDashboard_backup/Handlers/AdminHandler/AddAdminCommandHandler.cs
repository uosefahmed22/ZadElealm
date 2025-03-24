using AdminDashboard.Commands.AdminCommand;
using AdminDashboard.Controllers.AdminDashboard.Controllers;
using AdminDashboard.Dto;
using AdminDashboard.Helpers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.AdminHandler
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
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new AddAdminResult
                    {
                        Succeeded = false,
                        ErrorMessage = "User with this email already exists."
                    };
                }

                var adminUsersCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                if (adminUsersCount >= _adminSettings.Value.MaxAdminCount)
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
                if (!result.Succeeded)
                {
                    return new AddAdminResult
                    {
                        Succeeded = false,
                        Errors = result.Errors
                    };
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return new AddAdminResult
                    {
                        Succeeded = false,
                        ErrorMessage = "Failed to assign admin role."
                    };
                }

                return new AddAdminResult { Succeeded = true };
            }
            catch (Exception ex)
            {
                return new AddAdminResult
                {
                    Succeeded = false,
                    ErrorMessage = "An unexpected error occurred while creating the admin user."
                };
            }
        }
    }
}
