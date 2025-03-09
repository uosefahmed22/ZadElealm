using ZadElealm.Apis.Commands.Certificate;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.CertificateHandler
{
    public class GenerateCertificateCommandHandler : BaseCommandHandler<GenerateCertificateCommand, ApiResponse>
    {
        private readonly ICertificateService _certificateService;

        public GenerateCertificateCommandHandler(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        public override async Task<ApiResponse> Handle(GenerateCertificateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var certificate = await _certificateService.GenerateAndSaveCertificate(request.UserId, request.QuizId);
                return new ApiDataResponse(200,certificate);
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, ex.Message);
            }
        }
    }
}
