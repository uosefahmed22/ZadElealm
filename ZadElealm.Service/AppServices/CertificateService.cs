using QuestPDF.Fluent;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Assessment;
using ZadElealm.Service.Documents;

namespace ZadElealm.Service.AppServices;

public class CertificateService : ICertificateService
{
    private readonly IUnitOfWork _unitOfWork;

    public CertificateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

        var issuedAtUtc = DateTime.UtcNow;
        var certificateReference = CreateCertificateReference(issuedAtUtc);
        var fileName = GeneratePdfCertificate(
            user,
            quiz,
            progress.Score,
            issuedAtUtc,
            certificateReference);

        var certificate = new Certificate
        {
            Name = $"Certificate_{user.DisplayName}_{quiz.Name}",
            Description = $"شهادة اجتياز {quiz.Name} بدرجة {progress.Score}%",
            PdfUrl = fileName,
            UserId = userId,
            QuizId = quizId,
            CreatedAt = issuedAtUtc
        };

        return new ApiDataResponse(200, certificate, "تم إنشاء الشهادة بنجاح");
    }

    public async Task<ApiDataResponse> GenerateAndSaveAssessmentCertificate(
        string userId,
        int assessmentId)
    {
        if (string.IsNullOrWhiteSpace(userId) || assessmentId <= 0)
            return new ApiDataResponse(400, message: "بيانات المستخدم أو الاختبار غير صالحة");

        var progress = await _unitOfWork.Repository<AssessmentProgress>()
            .GetEntityWithSpecNoTrackingAsync(new AssessmentProgressSpecification(userId, assessmentId));
        if (progress == null)
            return new ApiDataResponse(404, message: "لم يتم العثور على نتيجة الاختبار");

        if (!progress.IsCompleted)
            return new ApiDataResponse(400, message: "لم يتم اجتياز الاختبار بعد");

        var issuedAtUtc = DateTime.UtcNow;
        var certificateReference = CreateCertificateReference(issuedAtUtc);
        var assessmentName = progress.Assessment.Name;
        var fileName = GeneratePdfCertificate(
            progress.AppUser.DisplayName,
            assessmentName,
            progress.Score,
            issuedAtUtc,
            certificateReference);

        var certificate = new Certificate
        {
            Name = $"Certificate_{progress.AppUser.DisplayName}_{assessmentName}",
            Description = $"شهادة اجتياز {assessmentName} بدرجة {progress.Score}%",
            PdfUrl = fileName,
            UserId = userId,
            AssessmentId = assessmentId,
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
        => GeneratePdfCertificate(
            user.DisplayName,
            quiz.Name,
            score,
            issuedAtUtc,
            certificateReference);

    private static string GeneratePdfCertificate(
        string studentName,
        string assessmentName,
        int score,
        DateTime issuedAtUtc,
        string certificateReference)
    {
        var certificatesDirectory = CertificateFileStorage.GetPrivateDirectory();
        Directory.CreateDirectory(certificatesDirectory);

        var fileName = $"certificate_{certificateReference.ToLowerInvariant()}.pdf";
        var filePath = CertificateFileStorage.GetPrivateFilePath(fileName);
        var logoPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "certificates",
            "logo.png");

        var model = new CertificateDocumentModel(
            studentName,
            assessmentName,
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
