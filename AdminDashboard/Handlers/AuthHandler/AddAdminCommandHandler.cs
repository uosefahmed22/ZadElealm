using AdminDashboard.Commands;
using AdminDashboard.Controllers.AdminDashboard.Controllers;
using AdminDashboard.Dto;
using AdminDashboard.Helpers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.AuthHandler
{
    public class AddAdminCommandHandler : IRequestHandler<AddAdminCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;

        public AddAdminCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse> Handle(AddAdminCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new ApiResponse(400, "User with this email already exists.");
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
                return new ApiResponse(400, "Failed to create user.");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return new ApiResponse(400, "Failed to assign admin role.");
            }
            return new ApiResponse(200, "Admin user created successfully.");
        }
    }
}
