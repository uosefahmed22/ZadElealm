using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Apis.Quaries.Certificate;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.CertificateHandler
{
    public class DownloadCertificateQueryHandler : BaseQueryHandler<DownloadCertificateQuery, FileResponseForCertificate>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DownloadCertificateQueryHandler(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public override async Task<FileResponseForCertificate> Handle(DownloadCertificateQuery request, CancellationToken cancellationToken)
        {
            var spec = new CertificateByIdSpecification(request.CertificateId);
            var certificate = await _unitOfWork.Repository<Core.Models.Certificate>()
                .GetEntityWithSpecAsync(spec);

            if (certificate == null)
                throw new Exception("الشهادة غير موجودة");

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, certificate.ImageUrl.TrimStart('/'));

            if (!File.Exists(filePath))
                throw new Exception("ملف الشهادة غير موجود");

            var fileContents = await File.ReadAllBytesAsync(filePath);

            return new FileResponseForCertificate
            {
                FileContents = fileContents,
                ContentType = "application/pdf",
                FileName = $"certificate_{certificate.Name}.pdf"
            };
        }
    }
}
