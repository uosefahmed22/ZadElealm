using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class VerifyOtpCommandHandler : BaseCommandHandler<VerifyOtpCommand, ApiResponse>
    {
        private readonly IOtpService _otpService;
        private readonly ILogger<VerifyOtpCommandHandler> _logger;

        public VerifyOtpCommandHandler(
            IOtpService otpService,
            ILogger<VerifyOtpCommandHandler> logger)
        {
            _otpService = otpService;
            _logger = logger;
        }

        public override async Task<ApiResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isValidOtp = await Task.Run(() =>
                    _otpService.IsValidOtp(request.Email, request.Otp));

                if (!isValidOtp)
                {
                    return new ApiResponse(400, "Invalid credentials.");
                }

                return new ApiResponse(200, "الرمز المؤقت صحيح.");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ غير متوقع");
            }
        }
    }
}
