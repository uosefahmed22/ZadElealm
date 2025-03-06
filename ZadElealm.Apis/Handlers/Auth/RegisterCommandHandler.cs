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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegisterCommandHandler(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ISendEmailService sendEmailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _sendEmailService = sendEmailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.RegisterDto.Email);
            if (existingUser != null)
            {
                return new ApiResponse(400, "المستخدم موجود بالفعل");
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
                return new ApiResponse(400, "حدث خطأ ما");
            }

            if (await _roleManager.FindByNameAsync("User") == null)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole("User"));
                if (!result.Succeeded)
                    return new ApiResponse(500, "فشل في إنشاء الدور");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!addToRoleResult.Succeeded)
                return new ApiResponse(500, "فشل في إضافة المستخدم إلى الدور");

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBackUrl = GenerateCallBackUrl(emailConfirmationToken, user.Id);

            var emailBody = $"<h1>عزيزي {user.DisplayName}</h1>" +
                            "<p>شكرًا لتسجيلك في موقعنا. يرجى تأكيد عنوان بريدك الإلكتروني بالنقر على الزر أدناه</p>" +
                            $"<a href='{callBackUrl}'><button style='background-color: #4CAF50; color: white; padding: 15px 32px; text-align: center; text-decoration: none; display: inline-block; font-size: 16px; margin: 4px 2px; cursor: pointer;'>تأكيد البريد الإلكتروني</button></a>";

            try
            {
                await _sendEmailService.SendEmailAsync(new EmailMessage
                {
                    To = user.Email,
                    Subject = "تأكيد البريد الإلكتروني",
                    Body = emailBody
                });
            }
            catch (Exception)
            {
                return new ApiResponse(400, "حدث خطأ أثناء إرسال رسالة التأكيد");
            }

            return new ApiResponse(200, "تم إنشاء الحساب بنجاح. يرجى التحقق من بريدك الإلكتروني لتأكيد الحساب");
        }

        private string GenerateCallBackUrl(string token, string userId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                throw new ArgumentNullException("Token and userId cannot be null or empty");

            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var encodedUserId = WebUtility.UrlEncode(userId);

            var request = _httpContextAccessor.HttpContext?.Request
                ?? throw new InvalidOperationException("HttpContext is not available");

            return new Uri(new Uri($"{request.Scheme}://{request.Host}"),
                $"/api/Account/confirm-email?userId={encodedUserId}&token={encodedToken}")
                .ToString();
        }
    }
}
