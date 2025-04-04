﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class ResetPasswordCommandHandler : BaseCommandHandler<ResetPasswordCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMemoryCache _cache;

        public ResetPasswordCommandHandler(UserManager<AppUser> userManager, IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.ResetPasswordDto.Email);
            if (user == null)
            {
                return new ApiResponse(404, "المستخدم غير موجود");
            }

            if (!_cache.TryGetValue(request.ResetPasswordDto.Email, out bool isOtpValid) || !isOtpValid)
            {
                return new ApiResponse(400, "رمز التحقق غير صالح أو منتهي الصلاحية");
            }

            var isOldPasswordEqualNew = await _userManager.CheckPasswordAsync(user, request.ResetPasswordDto.NewPassword);
            if (isOldPasswordEqualNew)
            {
                return new ApiResponse(400, "كلمة المرور الجديدة يجب أن تكون مختلفة عن كلمة المرور القديمة");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.ResetPasswordDto.NewPassword);

            _cache.Remove(request.ResetPasswordDto.Email);

            if (resetResult.Succeeded)
            {
                return new ApiResponse(200, "تم إعادة تعيين كلمة المرور بنجاح");
            }

            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            return new ApiResponse(400, $"فشل في إعادة تعيين كلمة المرور: {errors}");
        }
    }
}
