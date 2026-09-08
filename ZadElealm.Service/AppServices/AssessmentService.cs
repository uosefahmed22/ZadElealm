using System.Security.Cryptography;
using System.Text;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Policies;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Core.Specifications.Assessment;

namespace ZadElealm.Service.AppServices;

public sealed class AssessmentService : IAssessmentService
{
    private static readonly TimeSpan SubmissionGracePeriod = TimeSpan.FromSeconds(30);
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnrollmentReadRepository _enrollmentReadRepository;
    private readonly ICertificateService _certificateService;
    private readonly INotificationService _notificationService;
    private readonly ICertificateFileStorage _certificateFileStorage;

    public AssessmentService(
        IUnitOfWork unitOfWork,
        IEnrollmentReadRepository enrollmentReadRepository,
        ICertificateService certificateService,
        INotificationService notificationService,
        ICertificateFileStorage certificateFileStorage)
    {
        _unitOfWork = unitOfWork;
        _enrollmentReadRepository = enrollmentReadRepository;
        _certificateService = certificateService;
        _notificationService = notificationService;
        _certificateFileStorage = certificateFileStorage;
    }

    public async Task<ApiDataResponse> GetAvailableAssessmentsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new ApiDataResponse(400, null, "بيانات المستخدم غير صالحة");

        var assessments = await _unitOfWork.Repository<Assessment>()
            .GetAllWithSpecNoTrackingAsync(new ActiveAssessmentsSpecification());
        var progresses = await _unitOfWork.Repository<AssessmentProgress>()
            .GetAllWithSpecNoTrackingAsync(new AssessmentProgressSpecification(userId));
        var eligibleCategoryIds = await GetEligibleCategoryIdsAsync(userId);
        var progressByAssessment = progresses.ToDictionary(x => x.AssessmentId);

        var result = assessments.Select(assessment =>
        {
            progressByAssessment.TryGetValue(assessment.Id, out var progress);
            return new AssessmentSummaryDto
            {
                Id = assessment.Id,
                Name = assessment.Name,
                Description = assessment.Description,
                CategoryName = assessment.Category.Name,
                PassingScore = assessment.PassingScore,
                IsEligible = eligibleCategoryIds.Contains(assessment.CategoryId),
                IsCompleted = progress?.IsCompleted == true,
                BestScore = progress?.AttemptSubmittedAtUtc is null ? null : progress.Score
            };
        }).ToList();

        return new ApiDataResponse(200, result);
    }

    public async Task<ApiDataResponse> GetAssessmentAsync(string userId, int assessmentId)
    {
        var (assessment, error) = await LoadEligibleAssessmentAsync(userId, assessmentId);
        if (error != null)
            return error;

        var progress = await GetProgressAsync(userId, assessmentId);
        if (progress?.IsCompleted == true)
            return new ApiDataResponse(409, null, "تم اجتياز الاختبار مسبقًا");

        var form = SelectForm(assessment!, userId, progress?.AssessmentFormId);
        if (form == null)
            return new ApiDataResponse(409, null, "لا يوجد نموذج متاح للاختبار حاليًا");

        var now = DateTime.UtcNow;
        if (progress == null)
        {
            progress = new AssessmentProgress
            {
                AppUserId = userId,
                AssessmentId = assessmentId,
                AssessmentFormId = form.Id,
                Score = 0,
                AttemptStartedAtUtc = now,
                AttemptExpiresAtUtc = now.AddMinutes(assessment!.DurationMinutes),
                CreatedAt = now
            };
            await _unitOfWork.Repository<AssessmentProgress>().AddAsync(progress);
            await _unitOfWork.Complete();
        }
        else if (progress.AttemptStartedAtUtc == null ||
                 progress.AttemptExpiresAtUtc == null ||
                 progress.AttemptSubmittedAtUtc != null ||
                 progress.AttemptExpiresAtUtc <= now)
        {
            progress.AssessmentFormId = form.Id;
            progress.AttemptStartedAtUtc = now;
            progress.AttemptExpiresAtUtc = now.AddMinutes(assessment!.DurationMinutes);
            progress.AttemptSubmittedAtUtc = null;
            _unitOfWork.Repository<AssessmentProgress>().Update(progress);
            await _unitOfWork.Complete();
        }

        var dto = new AssessmentDto
        {
            Id = assessment!.Id,
            Name = assessment.Name,
            Description = assessment.Description,
            PassingScore = assessment.PassingScore,
            DurationMinutes = assessment.DurationMinutes,
            AttemptStartedAtUtc = progress.AttemptStartedAtUtc!.Value,
            AttemptExpiresAtUtc = progress.AttemptExpiresAtUtc!.Value,
            Questions = form.Questions
                .OrderBy(question => question.DisplayOrder)
                .Select(question => new AssessmentQuestionDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    Choices = question.Choices
                        .OrderBy(choice => choice.Id)
                        .Select(choice => new AssessmentChoiceDto
                        {
                            Id = choice.Id,
                            Text = choice.Text
                        }).ToList()
                }).ToList()
        };

        return new ApiDataResponse(200, dto);
    }

    public async Task<ApiDataResponse> SubmitAssessmentAsync(
        string userId,
        int assessmentId,
        AssessmentSubmissionDto submission)
    {
        if (submission?.StudentAnswers == null)
            return new ApiDataResponse(400, null, "إجابات الاختبار مطلوبة");

        var (assessment, error) = await LoadEligibleAssessmentAsync(userId, assessmentId);
        if (error != null)
            return error;

        var existingProgress = await GetProgressAsync(userId, assessmentId);
        var attemptError = ValidateAttempt(existingProgress, DateTime.UtcNow);
        if (attemptError != null)
            return attemptError;

        var form = SelectForm(assessment!, userId, existingProgress!.AssessmentFormId);
        if (form == null)
            return new ApiDataResponse(409, null, "لا يوجد نموذج متاح للاختبار حاليًا");

        var validationError = ValidateAnswers(form, submission.StudentAnswers);
        if (validationError != null)
            return validationError;

        var calculation = CalculateResult(
            form,
            submission.StudentAnswers,
            assessment!.PassingScore);

        await _unitOfWork.BeginTransactionAsync();
        string? generatedCertificateFile = null;
        try
        {
            await SaveSubmissionAsync(existingProgress, form.Id, calculation);

            if (calculation.IsCompleted)
            {
                var artifacts = await CreateCompletionArtifactsAsync(
                    userId,
                    assessmentId,
                    assessment.Name);
                generatedCertificateFile = artifacts.CertificateFileName;
                if (artifacts.Error != null)
                {
                    _certificateFileStorage.DeletePrivateFileIfExists(generatedCertificateFile);
                    return await RollbackAsync(artifacts.Error);
                }
            }

            await _unitOfWork.CommitTransactionAsync();
            return BuildSubmissionResponse(assessment.Name, existingProgress, calculation);
        }
        catch
        {
            _certificateFileStorage.DeletePrivateFileIfExists(generatedCertificateFile);
            await _unitOfWork.RollbackTransactionAsync();
            return new ApiDataResponse(500, null, "حدث خطأ أثناء حفظ نتيجة الاختبار");
        }
    }

    private static ApiDataResponse? ValidateAttempt(
        AssessmentProgress? progress,
        DateTime nowUtc)
    {
        if (progress?.IsCompleted == true)
            return new ApiDataResponse(409, null, "تم اجتياز الاختبار مسبقًا");
        if (progress?.AttemptStartedAtUtc == null || progress.AttemptExpiresAtUtc == null)
            return new ApiDataResponse(409, null, "ابدأ الاختبار أولًا قبل إرسال الإجابات");
        if (progress.AttemptSubmittedAtUtc != null)
            return new ApiDataResponse(409, null, "تم تسليم هذه المحاولة بالفعل");
        if (nowUtc > progress.AttemptExpiresAtUtc.Value.Add(SubmissionGracePeriod))
            return new ApiDataResponse(409, null, "انتهى وقت المحاولة. افتح الاختبار لبدء محاولة جديدة");

        return null;
    }

    private static AssessmentCalculation CalculateResult(
        AssessmentForm form,
        IReadOnlyList<StudentAnswerDto> answers,
        int passingScore)
    {
        var answersByQuestion = answers.ToDictionary(answer => answer.QuestionId);
        var correctAnswers = form.Questions.Count(question =>
            answersByQuestion.TryGetValue(question.Id, out var answer) &&
            question.Choices.Any(choice => choice.Id == answer.ChoiceId && choice.IsCorrect));
        var score = (correctAnswers * 100) / form.Questions.Count;

        return new AssessmentCalculation(
            score,
            score >= passingScore,
            form.Questions.Count,
            correctAnswers);
    }

    private async Task SaveSubmissionAsync(
        AssessmentProgress progress,
        int formId,
        AssessmentCalculation calculation)
    {
        progress.AssessmentFormId = formId;
        progress.Score = Math.Max(progress.Score, calculation.Score);
        progress.IsCompleted = progress.IsCompleted || calculation.IsCompleted;
        progress.AttemptSubmittedAtUtc = DateTime.UtcNow;
        _unitOfWork.Repository<AssessmentProgress>().Update(progress);
        await _unitOfWork.Complete();
    }

    private async Task<CompletionArtifactsResult> CreateCompletionArtifactsAsync(
        string userId,
        int assessmentId,
        string assessmentName)
    {
        var certificateResult = await _certificateService
            .GenerateAndSaveAssessmentCertificate(userId, assessmentId);
        if (certificateResult.StatusCode != 200 ||
            certificateResult.Data is not Certificate certificate)
        {
            return new CompletionArtifactsResult(
                new ApiDataResponse(500, null, "تعذر إنشاء شهادة الاختبار"),
                null);
        }

        try
        {
            await _unitOfWork.Repository<Certificate>().AddAsync(certificate);
            await _unitOfWork.Complete();

            var notificationResult = await _notificationService.SendNotificationAsync(
                BuildCertificateNotification(userId, assessmentName));
            var error = notificationResult.StatusCode == 200
                ? null
                : new ApiDataResponse(500, null, "تعذر إنشاء إشعار الشهادة");
            return new CompletionArtifactsResult(error, certificate.PdfUrl);
        }
        catch
        {
            _certificateFileStorage.DeletePrivateFileIfExists(certificate.PdfUrl);
            throw;
        }
    }

    private static NotificationServiceDto BuildCertificateNotification(
        string userId,
        string assessmentName)
        => new()
        {
            Title = "مبارك على اجتيازك!",
            Description = $"لقد اجتزت {assessmentName} بنجاح، وأصبحت شهادتك متاحة في قسم الشهادات.",
            Type = NotificationType.Certificate,
            UserId = userId
        };

    private async Task<ApiDataResponse> RollbackAsync(ApiDataResponse response)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return response;
    }

    private static ApiDataResponse BuildSubmissionResponse(
        string assessmentName,
        AssessmentProgress progress,
        AssessmentCalculation calculation)
        => new(200, new AssessmentResultDto
        {
            AssessmentName = assessmentName,
            Score = calculation.Score,
            IsCompleted = calculation.IsCompleted,
            TotalQuestions = calculation.TotalQuestions,
            CorrectAnswers = calculation.CorrectAnswers,
            Date = progress.AttemptSubmittedAtUtc!.Value
        }, calculation.IsCompleted
            ? "تهانينا! لقد اجتزت الاختبار بنجاح"
            : "تم تسليم الاختبار");

    private async Task<(Assessment? Assessment, ApiDataResponse? Error)> LoadEligibleAssessmentAsync(
        string userId,
        int assessmentId)
    {
        if (string.IsNullOrWhiteSpace(userId) || assessmentId <= 0)
            return (null, new ApiDataResponse(400, null, "بيانات الاختبار غير صالحة"));

        var assessment = await _unitOfWork.Repository<Assessment>()
            .GetEntityWithSpecNoTrackingAsync(new AssessmentWithFormsSpecification(assessmentId));
        if (assessment == null)
            return (null, new ApiDataResponse(404, null, "الاختبار غير موجود"));

        var eligibleCategoryIds = await GetEligibleCategoryIdsAsync(userId);
        return !eligibleCategoryIds.Contains(assessment.CategoryId)
            ? (null, new ApiDataResponse(403, null, "أكمل جميع دروس دورة واحدة في هذا التصنيف لفتح الاختبار"))
            : (assessment, null);
    }

    private async Task<HashSet<int>> GetEligibleCategoryIdsAsync(string userId)
    {
        var categoryIds = await _enrollmentReadRepository
            .GetCompletedCourseCategoryIdsAsync(userId);
        return categoryIds.ToHashSet();
    }

    private async Task<AssessmentProgress?> GetProgressAsync(string userId, int assessmentId)
        => await _unitOfWork.Repository<AssessmentProgress>()
            .GetEntityWithSpecAsync(new AssessmentProgressSpecification(userId, assessmentId));

    private static AssessmentForm? SelectForm(
        Assessment assessment,
        string userId,
        int? preferredFormId = null)
    {
        var forms = assessment.Forms
            .Where(form => form.IsActive &&
                form.Questions.Count > 0 &&
                form.Questions.All(question =>
                    question.Choices.Count >= 2 && question.Choices.Count(choice => choice.IsCorrect) == 1))
            .OrderBy(form => form.Id)
            .ToList();
        if (forms.Count == 0)
            return null;

        var preferredForm = preferredFormId.HasValue
            ? forms.FirstOrDefault(form => form.Id == preferredFormId.Value)
            : null;
        if (preferredForm != null)
            return preferredForm;

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{assessment.Id}:{userId}"));
        var index = (int)(BitConverter.ToUInt32(hash, 0) % (uint)forms.Count);
        return forms[index];
    }

    private static ApiDataResponse? ValidateAnswers(
        AssessmentForm form,
        IReadOnlyList<StudentAnswerDto> answers)
    {
        if (answers.Count > form.Questions.Count ||
            answers.Any(answer => answer.QuestionId <= 0 || answer.ChoiceId <= 0) ||
            answers.Select(answer => answer.QuestionId).Distinct().Count() != answers.Count)
        {
            return new ApiDataResponse(400, null, "إجابات الاختبار غير صالحة");
        }

        var questions = form.Questions.ToDictionary(question => question.Id);
        foreach (var answer in answers)
        {
            if (!questions.TryGetValue(answer.QuestionId, out var question) ||
                question.Choices.All(choice => choice.Id != answer.ChoiceId))
            {
                return new ApiDataResponse(400, null, "إحدى الإجابات لا تنتمي إلى هذا الاختبار");
            }
        }

        return null;
    }

    private sealed record AssessmentCalculation(
        int Score,
        bool IsCompleted,
        int TotalQuestions,
        int CorrectAnswers);

    private sealed record CompletionArtifactsResult(
        ApiDataResponse? Error,
        string? CertificateFileName);
}
