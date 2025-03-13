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

            if (await _roleManager.FindByNameAsync("User") == null)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("User"));
                if (!roleResult.Succeeded)
                    return new ApiResponse(500, "فشل في إنشاء الدور");
            }

            var user = new AppUser
            {
                DisplayName = request.RegisterDto.DisplayName,
                Email = request.RegisterDto.Email,
                UserName = request.RegisterDto.Email.Split('@')[0]
            };

            bool userCreated = false;

            try
            {
                var createUserResult = await _userManager.CreateAsync(user, request.RegisterDto.Password);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    return new ApiResponse(400, $"فشل في إنشاء الحساب: {errors}");
                }

                userCreated = true;

                var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    throw new Exception($"فشل في إضافة المستخدم إلى الدور: {errors}");
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callBackUrl = GenerateCallBackUrl(emailConfirmationToken, user.Id);

                var emailBody = $"<h1>عزيزي {user.DisplayName}</h1>" +
                    "مرحبًا بك في موقعنا, ونحن سعداء بانضمامك إلينا" +
                               "<p>شكرًا لتسجيلك في موقعنا. يرجى تأكيد عنوان بريدك الإلكتروني بالنقر على الزر أدناه</p>" +
                               $"<a href='{callBackUrl}'><button style='background-color: #4CAF50; color: white; padding: 15px 32px; text-align: center; text-decoration: none; display: inline-block; font-size: 16px; margin: 4px 2px; cursor: pointer;'>تأكيد البريد الإلكتروني</button></a>";

                await _sendEmailService.SendEmailAsync(new EmailMessage
                {
                    To = user.Email,
                    Subject = "تأكيد البريد الإلكتروني",
                    Body = emailBody
                });

                return new ApiResponse(200, "تم إنشاء الحساب بنجاح. يرجى التحقق من بريدك الإلكتروني لتأكيد الحساب");
            }
            catch (Exception ex)
            {
                if (userCreated)
                {
                    try
                    {
                        await _userManager.DeleteAsync(user);
                    }
                    catch
                    {
                        new ApiResponse(500, "هناك خطأ غير متوقع حدث ");
                    }
                }

                return new ApiResponse(400, "حدث خطأ أثناء إنشاء الحساب: ");
            }
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
