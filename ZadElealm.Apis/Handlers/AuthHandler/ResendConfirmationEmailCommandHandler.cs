﻿using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Text;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class ResendConfirmationEmailCommandHandler : BaseCommandHandler<ResendConfirmationEmailCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ISendEmailService _sendEmailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResendConfirmationEmailCommandHandler(
            UserManager<AppUser> userManager,
            ISendEmailService sendEmailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _sendEmailService = sendEmailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<ApiResponse> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return new ApiResponse(404, "المستخدم غير موجود");

            if (user.EmailConfirmed == true)
                return new ApiResponse(400, "البريد الإلكتروني مؤكد بالفعل");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = GenerateCallBackUrl(token, user.Id);

            var emailBody = $"<h1>عزيزي {user.DisplayName}</h1>" +
                "هذا البريد الإلكتروني تم إرساله لتأكيد بريدك الإلكتروني" +
                          "<p>لتأكيد بريدك الإلكتروني، اضغط على الرابط أدناه:</p>" +
                          $"<p><a href='{callbackUrl}'>اضغط هنا</a></p>";

            await _sendEmailService.SendEmailAsync(new EmailMessage
            {
                To = user.Email,
                Subject = "تأكيد البريد الإلكتروني",
                Body = emailBody
            });

            return new ApiResponse(200, "تم إرسال رسالة التأكيد بنجاح");
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