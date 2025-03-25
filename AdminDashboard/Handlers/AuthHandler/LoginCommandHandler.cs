using AdminDashboard.Commands;
using AdminDashboard.Controllers.AdminDashboard.Controllers;
using AdminDashboard.Dto;
using AdminDashboard.Quires;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.AuthHandler
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public LoginCommandHandler(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new LoginResult { Succeeded = false, ErrorMessage = "Invalid login attempt." };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return new LoginResult { Succeeded = false, ErrorMessage = "Invalid login attempt." };
            }

            return new LoginResult { Succeeded = true, User = user };
        }
    }
}
