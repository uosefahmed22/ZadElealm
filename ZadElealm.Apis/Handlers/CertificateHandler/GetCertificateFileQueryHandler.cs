using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.Certificate;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Certificate;
using ZadElealm.Service.Documents;

namespace ZadElealm.Apis.Handlers.CertificateHandler;

public sealed class GetCertificateFileQueryHandler
    : BaseQueryHandler<GetCertificateFileQuery, ApiResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCertificateFileQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public override async Task<ApiResponse> Handle(
        GetCertificateFileQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new CertificateByUserAndQuizSpecification(
            request.CertificateId,
            request.UserId);
        var certificate = await _unitOfWork.Repository<Certificate>()
            .GetEntityWithSpecNoTrackingAsync(specification);

        if (certificate is null)
            return new ApiDataResponse(404, message: "الشهادة غير موجودة");

        var filePath = CertificateFileStorage.ResolveExistingFile(certificate.PdfUrl);
        if (filePath is null)
            return new ApiDataResponse(404, message: "ملف الشهادة غير موجود");

        try
        {
            var content = await File.ReadAllBytesAsync(filePath, cancellationToken);
            return new ApiDataResponse(200, new CertificateFileDownloadDto(content));
        }
        catch (FileNotFoundException)
        {
            return new ApiDataResponse(404, message: "ملف الشهادة غير موجود");
        }
        catch (DirectoryNotFoundException)
        {
            return new ApiDataResponse(404, message: "ملف الشهادة غير موجود");
        }
    }
}
