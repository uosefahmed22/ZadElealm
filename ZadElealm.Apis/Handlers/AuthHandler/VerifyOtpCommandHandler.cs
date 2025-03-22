using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class VerifyOtpCommandHandler : BaseCommandHandler<VerifyOtpCommand, ApiResponse>
    {
        private readonly IOtpService _otpService;
        private readonly IMemoryCache _cache;

        public VerifyOtpCommandHandler(
            IOtpService otpService,
            IMemoryCache cache)
        {
            _otpService = otpService;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var isValidOtp = await Task.Run(() =>
                _otpService.IsValidOtp(request.Email, request.Otp));

            if (!isValidOtp)
            {
                return new ApiResponse(400, "الرمز المؤقت غير صحيح.");
            }

            _cache.Set(request.Email, true, TimeSpan.FromMinutes(15));

            return new ApiResponse(200, "تم التحقق من الرمز بنجاح");
        }
    }
}
