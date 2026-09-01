using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Service.Documents;

namespace ZadElealm.Service.AppServices;

public class CertificateService : ICertificateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public CertificateService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<ApiDataResponse> GenerateAndSaveCertificate(string userId, int quizId)
    {
        if (string.IsNullOrWhiteSpace(userId) || quizId <= 0)
            return new ApiDataResponse(400, message: "بيانات المستخدم أو الاختبار غير صالحة");

        var progressSpec = new ProgressWithUserDataAndQuiz(userId, quizId);
        var progress = await _unitOfWork.Repository<Progress>()
            .GetEntityWithSpecNoTrackingAsync(progressSpec);

        if (progress == null)
            return new ApiDataResponse(404, message: "لم يتم العثور على نتيجة الاختبار");

        if (!progress.IsCompleted)
            return new ApiDataResponse(400, message: "لم يتم إكمال الاختبار بعد");

        var user = progress.AppUser;
        var quiz = progress.Quiz;

        var baseUrl = _configuration["BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl) ||
            !Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            return new ApiDataResponse(500, message: "إعداد BaseUrl غير صالح");

        var issuedAtUtc = DateTime.UtcNow;
        var certificateReference = CreateCertificateReference(issuedAtUtc);
        var fileName = GeneratePdfCertificate(
            user,
            quiz,
            progress.Score,
            issuedAtUtc,
            certificateReference);

        var pdfUrl = $"{baseUrl}/certificates/{fileName}";

        var certificate = new Certificate
        {
            Name = $"Certificate_{user.DisplayName}_{quiz.Name}",
            Description = $"شهادة اجتياز {quiz.Name} بدرجة {progress.Score}%",
            PdfUrl = pdfUrl,
            UserId = userId,
            QuizId = quizId,
            CreatedAt = issuedAtUtc
        };

        return new ApiDataResponse(200, certificate, "تم إنشاء الشهادة بنجاح");
    }

    private static string CreateCertificateReference(DateTime issuedAtUtc)
    {
        var randomPart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"ZA-{issuedAtUtc:yyyy}-{randomPart}";
    }

    private static string GeneratePdfCertificate(
        AppUser user,
        Quiz quiz,
        int score,
        DateTime issuedAtUtc,
        string certificateReference)
    {
        var certificatesDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "certificates");
        Directory.CreateDirectory(certificatesDirectory);

        var fileName = $"certificate_{certificateReference.ToLowerInvariant()}.pdf";
        var filePath = Path.Combine(certificatesDirectory, fileName);
        var logoPath = Path.Combine(certificatesDirectory, "logo.png");

        var model = new CertificateDocumentModel(
            user.DisplayName,
            quiz.Name,
            score,
            issuedAtUtc,
            certificateReference,
            logoPath);

        new CertificateDocument(model).GeneratePdf(filePath);

        if (!File.Exists(filePath))
            throw new IOException("فشل في إنشاء ملف PDF");

        return fileName;
    }
}
