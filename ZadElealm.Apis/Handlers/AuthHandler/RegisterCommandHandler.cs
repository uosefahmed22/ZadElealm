using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Text;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Helpers;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class RegisterCommandHandler : BaseCommandHandler<RegisterCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISendEmailService _sendEmailService;
        private readonly IConfiguration _configuration;

        public RegisterCommandHandler(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ISendEmailService sendEmailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _sendEmailService = sendEmailService;
            _configuration = configuration;
        }

        public override async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.RegisterDto.Email);
            if (existingUser != null)
            {
                return new ApiResponse(400, "المستخدم موجود بالفعل");
            }

            var userRole = await _roleManager.FindByNameAsync("User");
            if (userRole == null)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("User"));
                if (!roleResult.Succeeded)
                {
                    return new ApiResponse(500, "فشل في إنشاء الدور");
                }
            }

            var user = new AppUser
            {
                DisplayName = request.RegisterDto.DisplayName,
                Email = request.RegisterDto.Email,
                UserName = request.RegisterDto.Email.Split('@')[0]
            };

            var createUserResult = await _userManager.CreateAsync(user, request.RegisterDto.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                return new ApiResponse(400, $"فشل في إنشاء الحساب: {errors}");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!addToRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return new ApiResponse(400, "فشل في إضافة المستخدم إلى الدور");
            }

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBackUrl = GenerateCallBackUrl(emailConfirmationToken, user.Id);

            var emailMessage = new EmailMessage
            {
                To = user.Email,
                Subject = "تأكيد البريد الإلكتروني",
                Body = AccountEmailTemplates.WelcomeConfirmation(user.DisplayName, callBackUrl)
            };

            var emailSent = await _sendEmailService.SendEmailAsync(emailMessage, cancellationToken);

            if (emailSent.StatusCode != 200)
            {
                await _userManager.DeleteAsync(user);
                return new ApiResponse(500, "فشل في إرسال البريد الإلكتروني للتأكيد");
            }

            return new ApiResponse(200, "تم إنشاء الحساب بنجاح. يرجى التحقق من بريدك الإلكتروني لتأكيد الحساب");
        }

        private string GenerateCallBackUrl(string token, string userId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                return string.Empty;
            }

            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var encodedUserId = WebUtility.UrlEncode(userId);

            var apiBaseUrl = _configuration["BaseUrl"] ?? "https://zadelealm.runasp.net";
            return AuthUrlBuilder.BuildConfirmEmailUrl(apiBaseUrl, encodedUserId, encodedToken);
        }
    }
}
