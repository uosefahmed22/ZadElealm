using ZadElealm.Apis.Commands.Certificate;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications.Certificate;

namespace ZadElealm.Apis.Handlers.CertificateHandler
{
    public class GenerateCertificateCommandHandler : BaseCommandHandler<GenerateCertificateCommand, ApiResponse>
    {
        private readonly ICertificateService _certificateService;
        private readonly IUnitOfWork _unitOfWork;

        public GenerateCertificateCommandHandler(
            ICertificateService certificateService,
            IUnitOfWork unitOfWork)
        {
            _certificateService = certificateService;
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(GenerateCertificateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var certSpec = new CertificateByUserAndQuizSpecification(request.UserId, request.QuizId);
                var existingCertificate = await _unitOfWork.Repository<Core.Models.Certificate>()
                    .GetEntityWithSpecAsync(certSpec);

                if (existingCertificate != null)
                {
                    var existingResponse = new CertificateResponseDto
                    {
                        StudentName = existingCertificate.User.DisplayName,
                        QuizName = existingCertificate.Quiz.Name,
                        PdfUrl = existingCertificate.PdfUrl,
                        CreatedAt = existingCertificate.CreatedAt
                    };
                    return new ApiDataResponse(200, existingResponse, "تم العثور على الشهادة");
                }

                var certificate = await _certificateService.GenerateAndSaveCertificate(request.UserId, request.QuizId);

                await _unitOfWork.Repository<Core.Models.Certificate>().AddAsync(certificate);
                await _unitOfWork.Complete();

                var response = new CertificateResponseDto
                {
                    StudentName = certificate.User.DisplayName,
                    QuizName = certificate.Quiz.Name,
                    PdfUrl = certificate.PdfUrl,
                    CreatedAt = certificate.CreatedAt
                };

                return new ApiDataResponse(200, response, "تم إنشاء الشهادة بنجاح");
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, ex.Message);
            }
        }
    }
}
