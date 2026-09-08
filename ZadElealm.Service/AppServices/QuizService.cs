using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Quiz;
using ZadElealm.Core.Enums;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Core.Errors;

namespace ZadElealm.Service.AppServices
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICertificateService _certificateService;
        private readonly IVideoProgressService _videoProgressService;
        private readonly INotificationService _notificationService;
        private readonly ICertificateFileStorage _certificateFileStorage;

        public QuizService(IUnitOfWork unitOfWork,
            ICertificateService certificateService,
            IVideoProgressService videoProgressService,
            INotificationService notificationService,
            ICertificateFileStorage certificateFileStorage)
        {
            _unitOfWork = unitOfWork;
            _certificateService = certificateService;
            _videoProgressService = videoProgressService;
            _notificationService = notificationService;
            _certificateFileStorage = certificateFileStorage;
        }

        public async Task<ApiResponse> CreateQuizAsync(CreateQuizDto quizDto)
        {
            var validationError = ValidateCreateQuiz(quizDto);
            if (validationError != null)
                return validationError;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await PersistQuizAsync(quizDto);
                await _unitOfWork.CommitTransactionAsync();
                return new ApiResponse(200, "تم إنشاء الاختبار بنجاح");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ApiResponse(500, "حدث خطأ أثناء إنشاء الاختبار");
            }
        }

        public async Task<ApiDataResponse> SubmitQuizAsync(string userId, QuizSubmissionDto submission)
        {
            var validationError = ValidateSubmission(userId, submission);
            if (validationError != null)
                return validationError;

            var (context, preparationError) = await PrepareSubmissionAsync(userId, submission);
            if (preparationError != null)
                return preparationError;

            await _unitOfWork.BeginTransactionAsync();
            string? generatedCertificateFile = null;
            try
            {
                var progress = await SaveProgressAsync(
                    userId,
                    submission.QuizId,
                    context!.ExistingProgress,
                    context.Calculation);

                if (context.Calculation.IsCompleted)
                {
                    var completion = await CreateCompletionArtifactsAsync(userId, submission.QuizId);
                    generatedCertificateFile = completion.CertificateFileName;
                    if (completion.Error != null)
                    {
                        _certificateFileStorage.DeletePrivateFileIfExists(generatedCertificateFile);
                        await _unitOfWork.RollbackTransactionAsync();
                        return completion.Error;
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
                return BuildSubmissionResponse(context.Quiz, progress, context.Calculation);
            }
            catch
            {
                _certificateFileStorage.DeletePrivateFileIfExists(generatedCertificateFile);
                await _unitOfWork.RollbackTransactionAsync();
                return new ApiDataResponse(500, null, "حدث خطأ أثناء حفظ النتائج");
            }
        }

        private static ApiResponse? ValidateCreateQuiz(CreateQuizDto quizDto)
        {
            if (quizDto == null)
                return new ApiResponse(400, "بيانات الاختبار مطلوبة");

            if (quizDto.CourseId <= 0 || string.IsNullOrWhiteSpace(quizDto.Name) ||
                string.IsNullOrWhiteSpace(quizDto.Description) ||
                quizDto.Questions == null || quizDto.Questions.Count == 0)
                return new ApiResponse(400, "بيانات الاختبار غير مكتملة");

            foreach (var question in quizDto.Questions)
            {
                var questionError = ValidateQuestion(question);
                if (questionError != null)
                    return questionError;
            }

            return null;
        }

        private static ApiResponse? ValidateQuestion(CreateQuestionDto question)
        {
            if (question == null || string.IsNullOrWhiteSpace(question.Text) ||
                question.Choices == null || question.Choices.Count < 2)
                return new ApiResponse(400, "يجب أن يحتوي كل سؤال على خيارين على الأقل");

            if (question.CorrectChoiceIndex < 0 || question.CorrectChoiceIndex >= question.Choices.Count)
                return new ApiResponse(400, "الإجابة الصحيحة غير موجودة في الخيارات");

            return question.Choices.Any(choice => choice == null || string.IsNullOrWhiteSpace(choice.Text))
                ? new ApiResponse(400, "نصوص الخيارات مطلوبة")
                : null;
        }

        private async Task PersistQuizAsync(CreateQuizDto quizDto)
        {
            var quiz = BuildQuiz(quizDto);

            await _unitOfWork.Repository<Quiz>().AddAsync(quiz);
            await _unitOfWork.Complete();

            SetCorrectChoices(quiz, quizDto.Questions);
            await _unitOfWork.Complete();
        }

        private static Quiz BuildQuiz(CreateQuizDto quizDto)
            => new()
            {
                Name = quizDto.Name,
                Description = quizDto.Description,
                CourseId = quizDto.CourseId,
                Questions = quizDto.Questions.Select(questionDto => new Question
                {
                    Text = questionDto.Text,
                    Choices = questionDto.Choices.Select(choiceDto => new Choice
                    {
                        Text = choiceDto.Text
                    }).ToList()
                }).ToList()
            };

        private static void SetCorrectChoices(Quiz quiz, IReadOnlyList<CreateQuestionDto> questions)
        {
            for (var index = 0; index < questions.Count; index++)
            {
                var correctChoiceIndex = questions[index].CorrectChoiceIndex;
                quiz.Questions[index].CorrectChoiceId = quiz.Questions[index].Choices[correctChoiceIndex].Id;
            }
        }

        private static ApiDataResponse? ValidateSubmission(string userId, QuizSubmissionDto submission)
        {
            if (string.IsNullOrWhiteSpace(userId) || submission == null || submission.QuizId <= 0)
                return new ApiDataResponse(400, null, "بيانات تسليم الاختبار غير صالحة");

            submission.StudentAnswers ??= new List<StudentAnswerDto>();

            if (submission.StudentAnswers.Any(answer =>
                    answer == null || answer.QuestionId <= 0 || answer.ChoiceId <= 0))
                return new ApiDataResponse(400, null, "بيانات الإجابات غير صالحة");

            var duplicateQuestions = submission.StudentAnswers
                .GroupBy(answer => answer.QuestionId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            return duplicateQuestions.Count > 0
                ? new ApiDataResponse(400, null,
                    $"تم تقديم إجابتين أو أكثر لنفس السؤال: {string.Join(", ", duplicateQuestions)}")
                : null;
        }

        private async Task<(SubmissionContext? Context, ApiDataResponse? Error)> PrepareSubmissionAsync(
            string userId,
            QuizSubmissionDto submission)
        {
            var quiz = await GetQuizAsync(submission.QuizId);
            var answersError = ValidateQuizAnswers(quiz, submission.StudentAnswers);
            if (answersError != null)
                return (null, answersError);

            var existingProgress = await GetProgressAsync(submission.QuizId, userId);
            if (existingProgress?.IsCompleted == true)
                return (null, new ApiDataResponse(400, null, "تم إكمال الاختبار مسبقاً"));

            var isEligible = await _videoProgressService
                .CheckCourseCompletionEligibilityAsync(userId, quiz!.CourseId);
            if (!isEligible)
                return (null, new ApiDataResponse(403, null,
                    "الرجاء إكمال جميع دروس الدورة للدخول للاختبار"));

            var calculation = CalculateResult(quiz, submission.StudentAnswers);
            return (new SubmissionContext(quiz, existingProgress, calculation), null);
        }

        private async Task<Quiz?> GetQuizAsync(int quizId)
        {
            var specification = new QuizWithQuestionsAndChoicesSpecification(quizId);
            return await _unitOfWork.Repository<Quiz>()
                .GetEntityWithSpecNoTrackingAsync(specification);
        }

        private static ApiDataResponse? ValidateQuizAnswers(
            Quiz? quiz,
            IReadOnlyCollection<StudentAnswerDto> answers)
        {
            if (quiz == null)
                return new ApiDataResponse(404, null, "الاختبار غير موجود");

            if (quiz.Questions == null || quiz.Questions.Count == 0)
                return new ApiDataResponse(400, null, "الاختبار لا يحتوي على أسئلة");

            var questionsById = quiz.Questions.ToDictionary(question => question.Id);
            foreach (var answer in answers)
            {
                if (!questionsById.TryGetValue(answer.QuestionId, out var question))
                    return new ApiDataResponse(400, null, $"السؤال غير موجود: {answer.QuestionId}");

                if (question.Choices == null || question.Choices.All(choice => choice.Id != answer.ChoiceId))
                    return new ApiDataResponse(400, null, $"الخيار غير صالح للسؤال: {answer.QuestionId}");
            }

            return null;
        }

        private async Task<Progress?> GetProgressAsync(int quizId, string userId)
        {
            var specification = new ProgressByQuizAndUserSpecification(quizId, userId);
            return await _unitOfWork.Repository<Progress>().GetEntityWithSpecAsync(specification);
        }

        private static QuizCalculation CalculateResult(
            Quiz quiz,
            IReadOnlyCollection<StudentAnswerDto> answers)
        {
            var answerMap = answers.ToDictionary(answer => answer.QuestionId);
            var questionResults = new List<QuestionResultDto>(quiz.Questions.Count);
            var correctAnswers = 0;

            foreach (var question in quiz.Questions)
            {
                var hasAnswer = answerMap.TryGetValue(question.Id, out var answer);
                var isCorrect = hasAnswer && question.CorrectChoiceId == answer!.ChoiceId;
                if (isCorrect)
                    correctAnswers++;

                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.Text,
                    IsCorrect = isCorrect,
                    SelectedChoice = hasAnswer ? answer!.ChoiceId : 0,
                    CorrectChoice = question.CorrectChoiceId
                });
            }

            var totalQuestions = quiz.Questions.Count;
            var score = (correctAnswers * 100) / totalQuestions;

            return new QuizCalculation(
                score,
                score >= 60,
                totalQuestions,
                correctAnswers,
                totalQuestions - answerMap.Count,
                questionResults);
        }

        private async Task<Progress> SaveProgressAsync(
            string userId,
            int quizId,
            Progress? existingProgress,
            QuizCalculation calculation)
        {
            var progress = existingProgress ?? new Progress
            {
                QuizId = quizId,
                AppUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            progress.Score = Math.Max(progress.Score, calculation.Score);
            progress.IsCompleted = progress.IsCompleted || calculation.IsCompleted;

            if (existingProgress == null)
                await _unitOfWork.Repository<Progress>().AddAsync(progress);
            else
                _unitOfWork.Repository<Progress>().Update(progress);

            await _unitOfWork.Complete();
            return progress;
        }

        private async Task<CompletionArtifactsResult> CreateCompletionArtifactsAsync(
            string userId,
            int quizId)
        {
            var certificateResult = await _certificateService.GenerateAndSaveCertificate(userId, quizId);
            if (certificateResult.StatusCode != 200 || certificateResult.Data is not Certificate certificate)
                return new CompletionArtifactsResult(
                    new ApiDataResponse(500, null, "حدث خطأ أثناء إنشاء الشهادة"),
                    null);

            try
            {
                var notificationResult = await _notificationService.SendNotificationAsync(
                    BuildCertificateNotification(userId));
                if (notificationResult.StatusCode != 200)
                {
                    return new CompletionArtifactsResult(
                        new ApiDataResponse(500, null, "حدث خطأ أثناء إنشاء الإشعار"),
                        certificate.PdfUrl);
                }

                await _unitOfWork.Repository<Certificate>().AddAsync(certificate);
                await _unitOfWork.Complete();
                return new CompletionArtifactsResult(null, certificate.PdfUrl);
            }
            catch
            {
                _certificateFileStorage.DeletePrivateFileIfExists(certificate.PdfUrl);
                throw;
            }
        }

        private static NotificationServiceDto BuildCertificateNotification(string userId)
            => new()
            {
                Title = "مبارك على اجتيازك!",
                Description = "الحمد لله، لقد اجتزت الامتحان بنجاح! نسأل الله أن يبارك لك في علمك وعملك، وأن يجعلك من النافعين لدينك وأمتك. يمكنك الآن استلام شهادتك من قسم الشهادات. نسأل الله لك التوفيق والسداد في مسيرتك العلمية.",
                Type = NotificationType.Certificate,
                UserId = userId
            };

        private static ApiDataResponse BuildSubmissionResponse(
            Quiz quiz,
            Progress progress,
            QuizCalculation calculation)
        {
            var result = new QuizResultDto
            {
                QuizName = quiz.Name,
                Score = calculation.Score,
                IsCompleted = calculation.IsCompleted,
                TotalQuestions = calculation.TotalQuestions,
                CorrectAnswers = calculation.CorrectAnswers,
                UnansweredQuestions = calculation.UnansweredQuestions,
                Date = progress.CreatedAt,
                QuestionResults = calculation.QuestionResults
            };

            var message = calculation.IsCompleted
                ? "تهانينا! لقد اجتزت الاختبار بنجاح"
                : "تم تسليم الاختبار";

            return new ApiDataResponse(200, result, message);
        }

        private sealed record SubmissionContext(
            Quiz Quiz,
            Progress? ExistingProgress,
            QuizCalculation Calculation);

        private sealed record QuizCalculation(
            int Score,
            bool IsCompleted,
            int TotalQuestions,
            int CorrectAnswers,
            int UnansweredQuestions,
            List<QuestionResultDto> QuestionResults);

        private sealed record CompletionArtifactsResult(
            ApiDataResponse? Error,
            string? CertificateFileName);
    }
}
