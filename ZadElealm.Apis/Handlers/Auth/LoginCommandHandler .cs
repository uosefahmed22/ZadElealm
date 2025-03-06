using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class LoginCommandHandler : BaseCommandHandler<LoginCommand, ApiResponse>
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;

        public LoginCommandHandler(
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _tokenService = tokenService;
            _userManager = userManager;
        }

        public override async Task<ApiResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.LoginDto.Email);
            if (user == null)
                return new ApiResponse(404, "المستخدم غير موجود");

            if (!user.EmailConfirmed)
                return new ApiResponse(401, "لم يتم تأكيد البريد الإلكتروني");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.LoginDto.Password, false);
            if (!result.Succeeded)
                return new ApiResponse(401, "كلمة المرور غير صحيحة");

            var userDto = await _tokenService.CreateToken(user);
            return new ApiDataResponse(200,userDto);
        }
    }
}
