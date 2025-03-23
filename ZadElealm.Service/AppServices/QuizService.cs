using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Quiz;
using ZadElealm.Core.Enums;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Service.AppServices
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICertificateService _certificateService;
        private readonly IVideoProgressService _videoProgressService;
        private readonly INotificationService _notificationService;

        public QuizService(IUnitOfWork unitOfWork,
            ICertificateService certificateService,
            IVideoProgressService videoProgressService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _certificateService = certificateService;
            _videoProgressService = videoProgressService;
            _notificationService = notificationService;
        }
        public async Task<ApiResponse> CreateQuizAsync(CreateQuizDto quizDto)
        {
            if (quizDto == null)
                return new ApiResponse(400, "بيانات الاختبار مطلوبة");

            foreach (var question in quizDto.Questions)
            {
                if (question.CorrectChoiceIndex >= question.Choices.Count)
                    return new ApiResponse(400, "الإجابة الصحيحة غير موجودة في الخيارات");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Create quiz first
                var quiz = new Quiz
                {
                    Name = quizDto.Name,
                    Description = quizDto.Description,
                    CourseId = quizDto.CourseId,
                    Questions = new List<Question>()
                };

                // Add quiz to get its ID
                await _unitOfWork.Repository<Quiz>().AddAsync(quiz);
                await _unitOfWork.Complete();

                // Now create questions and choices
                for (int i = 0; i < quizDto.Questions.Count; i++)
                {
                    var questionDto = quizDto.Questions[i];
                    var question = new Question
                    {
                        Text = questionDto.Text,
                        QuizId = quiz.Id,
                        Choices = new List<Choice>()
                    };

                    // Add question to get its ID
                    quiz.Questions.Add(question);
                    await _unitOfWork.Complete();

                    // Create choices
                    var choices = questionDto.Choices.Select(c => new Choice
                    {
                        Text = c.Text,
                        QuestionId = question.Id
                    }).ToList();

                    // Add choices
                    question.Choices.AddRange(choices);
                    await _unitOfWork.Complete();

                    // Now we can set the correct choice ID
                    question.CorrectChoiceId = choices[questionDto.CorrectChoiceIndex].Id;
                    await _unitOfWork.Complete();
                }

                await _unitOfWork.CommitTransactionAsync();
                return new ApiResponse(200, "تم إنشاء الاختبار بنجاح");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ApiResponse(500, "حدث خطأ أثناء إنشاء الاختبار");
            }
        }
        public async Task<ApiDataResponse> SubmitQuizAsync(string userId, QuizSubmissionDto submission)
        {
            try
            {
                var duplicateQuestions = submission.StudentAnswers
                    .GroupBy(a => a.QuestionId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateQuestions.Any())
                    return new ApiDataResponse(400, null, $"تم تقديم إجابتين أو أكثر لنفس السؤال: {string.Join(", ", duplicateQuestions)}");

                var quizspec = new QuizWithQuestionsAndChoicesSpecification(submission.QuizId);
                var quiz = await _unitOfWork.Repository<Quiz>()
                    .GetEntityWithSpecAsync(quizspec);

                if (quiz == null)
                    return new ApiDataResponse(404, null, "الاختبار غير موجود");

                var questionIds = quiz.Questions.Select(q => q.Id).ToHashSet();
                foreach (var answer in submission.StudentAnswers)
                {
                    if (!questionIds.Contains(answer.QuestionId))
                        return new ApiDataResponse(400, null, $"السؤال غير موجود: {answer.QuestionId}");
                }

                var spec = new ProgressByQuizAndUserSpecification(submission.QuizId, userId);
                var existingProgress = await _unitOfWork.Repository<Progress>()
                    .GetEntityWithSpecAsync(spec);

                if (existingProgress?.IsCompleted == true)
                    return new ApiDataResponse(400, null, "تم إكمال الاختبار مسبقاً");

                var answerMap = submission.StudentAnswers.DistinctBy(a => a.QuestionId).ToDictionary(a => a.QuestionId);
                var questionResults = new List<QuestionResultDto>();
                int correctAnswers = 0;

                foreach (var question in quiz.Questions)
                {
                    bool hasAnswer = answerMap.TryGetValue(question.Id, out var answer);
                    bool isCorrect = hasAnswer && question.CorrectChoiceId == answer!.ChoiceId;
                    if (isCorrect) correctAnswers++;

                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        IsCorrect = isCorrect,
                        SelectedChoice = hasAnswer ? answer!.ChoiceId : 0,
                        CorrectChoice = question.CorrectChoiceId
                    });
                }

                int totalQuestions = quiz.Questions.Count;
                int score = totalQuestions > 0 ? (correctAnswers * 100) / totalQuestions : 0;
                int unansweredCount = totalQuestions - submission.StudentAnswers.Count;
                bool isCompleted = score >= 60;

                await _unitOfWork.BeginTransactionAsync();

                Progress progress;
                if (existingProgress != null)
                {
                    existingProgress.Score = Math.Max(existingProgress.Score, score);
                    existingProgress.IsCompleted = existingProgress.IsCompleted || isCompleted;
                    progress = existingProgress;
                    _unitOfWork.Repository<Progress>().Update(existingProgress);
                }
                else
                {
                    progress = new Progress
                    {
                        QuizId = submission.QuizId,
                        AppUserId = userId,
                        Score = score,
                        IsCompleted = isCompleted,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Progress>().AddAsync(progress);
                }

                await _unitOfWork.Complete();

                if (isCompleted && (!existingProgress?.IsCompleted ?? true))
                {
                    var certificateResult = await _certificateService.GenerateAndSaveCertificate(userId, submission.QuizId);
                    if (certificateResult.StatusCode != 200)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiDataResponse(500, null, "حدث خطأ أثناء إنشاء الشهادة");
                    }

                    var notificationDto = new NotificationServiceDto
                    {
                        Title = "مبارك على اجتيازك!",
                        Description = "الحمد لله، لقد اجتزت الامتحان بنجاح! نسأل الله أن يبارك لك في علمك وعملك، وأن يجعلك من النافعين لدينك وأمتك. يمكنك الآن استلام شهادتك من قسم الشهادات. نسأل الله لك التوفيق والسداد في مسيرتك العلمية.",
                        Type = NotificationType.Certificate,
                        UserId = userId
                    };

                    await _notificationService.SendNotificationAsync(notificationDto);
                    await _unitOfWork.Repository<Certificate>().AddAsync((Certificate)certificateResult.Data);
                    await _unitOfWork.Complete();
                }

                await _unitOfWork.CommitTransactionAsync();

                var result = new QuizResultDto
                {
                    QuizName = quiz.Name,
                    Score = score,
                    IsCompleted = isCompleted,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswers,
                    UnansweredQuestions = unansweredCount,
                    Date = progress.CreatedAt,
                    QuestionResults = questionResults
                };

                return new ApiDataResponse(200, result, isCompleted ? "تهانينا! لقد اجتزت الاختبار بنجاح" : "تم تسليم الاختبار");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ApiDataResponse(500, null, $"حدث خطأ أثناء حفظ النتائج: {ex.Message}");
            }
        }
    }
}