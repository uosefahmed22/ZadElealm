using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Text;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class RegisterCommandHandler : BaseCommandHandler<RegisterCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISendEmailService _sendEmailService;

        public RegisterCommandHandler(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ISendEmailService sendEmailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _sendEmailService = sendEmailService;
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

            var emailBody = BuildEmailBody(user.DisplayName, callBackUrl);
            var emailMessage = new EmailMessage
            {
                To = user.Email,
                Subject = "تأكيد البريد الإلكتروني",
                Body = emailBody
            };

            var emailSent = await _sendEmailService.SendEmailAsync(emailMessage);

            if (emailSent.StatusCode != 200)
            {
                await _userManager.DeleteAsync(user);
                return new ApiResponse(500, "فشل في إرسال البريد الإلكتروني للتأكيد");
            }

            return new ApiResponse(200, "تم إنشاء الحساب بنجاح. يرجى التحقق من بريدك الإلكتروني لتأكيد الحساب");
        }

        private string BuildEmailBody(string displayName, string callBackUrl)
        {
            return $@"<h1>عزيزي {displayName}</h1>
              مرحبًا بك في موقعنا, ونحن سعداء بانضمامك إلينا
              <p>شكرًا لتسجيلك في موقعنا. يرجى تأكيد عنوان بريدك الإلكتروني بالنقر على الزر أدناه</p>
              <a href='{callBackUrl}'><button style='background-color: #4CAF50; color: white; padding: 15px 32px; text-align: center; text-decoration: none; display: inline-block; font-size: 16px; margin: 4px 2px; cursor: pointer;'>تأكيد البريد الإلكتروني</button></a>";
        }
        private string GenerateCallBackUrl(string token, string userId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                return string.Empty;
            }

            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var encodedUserId = WebUtility.UrlEncode(userId);

            return $"https://zadelealm.runasp.net/api/Account/confirm-email?userId={encodedUserId}&token={encodedToken}";
        }
    }
}
