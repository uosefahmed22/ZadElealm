using Microsoft.AspNetCore.Identity;
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

        public ResetPasswordCommandHandler(
            UserManager<AppUser> userManager,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.ResetPasswordDto.Email);
                if (user == null)
                {
                    return new ApiResponse(400, "المستخدم غير موجود");
                }

                if (!_cache.TryGetValue(request.ResetPasswordDto.Email, out bool isOtpValid) || !isOtpValid)
                {
                    return new ApiResponse(400, "رمز التحقق غير صالح أو منتهي الصلاحية");
                }
                _cache.Remove(request.ResetPasswordDto.Email);

                var isOldPasswordEqualNew = await _userManager.CheckPasswordAsync(user, request.ResetPasswordDto.NewPassword);
                if (isOldPasswordEqualNew)
                {
                    return new ApiResponse(400, "كلمة المرور الجديدة يجب أن تكون مختلفة عن كلمة المرور القديمة");
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.ResetPasswordDto.NewPassword);

                if (resetResult.Succeeded)
                {
                    return new ApiResponse(200, "تم إعادة تعيين كلمة المرور بنجاح");
                }

                return new ApiResponse(500, $"فشل في إعادة تعيين كلمة المرور");
            }
            catch
            {
                return new ApiResponse(500, "حدث خطأ أثناء معالجة طلبك");
            }
        }
    }
}
