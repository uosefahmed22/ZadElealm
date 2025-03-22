using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.Auth
{
    public class VerifyOtpCommandHandler : BaseCommandHandler<VerifyOtpCommand, ApiResponse>
    {
        private readonly IOtpService _otpService;

        public VerifyOtpCommandHandler(
            IOtpService otpService)
        {
            _otpService = otpService;
        }

        public override async Task<ApiResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {

            var isValidOtp = await Task.Run(() =>
                _otpService.IsValidOtp(request.Email, request.Otp));

            if (!isValidOtp)
            {
                return new ApiResponse(400, "الرمز المؤقت غير صحيح.");
            }

            return new ApiResponse(200, "الرمز المؤقت صحيح.");
        }
    }
}
