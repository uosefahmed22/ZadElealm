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
        public async Task<ApiResponse> CreateQuizAsync(QuizDto quizDto)
        {
            try
            {
                if (quizDto == null || !quizDto.Questions.Any())
                    return new ApiResponse(400, "بيانات الاختبار غير صحيحة");
               
                var quiz = new Quiz
                {
                    Name = quizDto.Name,
                    Description = quizDto.Description,
                    CourseId = quizDto.CourseId,
                    Questions = new List<Question>()
                };

                foreach (var questionDto in quizDto.Questions)
                {
                    var question = new Question
                    {
                        Text = questionDto.Text,
                        Choices = new List<Choice>()
                    };

                    foreach (var choiceDto in questionDto.Choices)
                    {
                        var choice = new Choice
                        {
                            Text = choiceDto.Text,
                            Question = question
                        };
                        question.Choices.Add(choice);
                    }

                    quiz.Questions.Add(question);
                }

                await _unitOfWork.Repository<Quiz>().AddAsync(quiz);
                await _unitOfWork.Complete();

                foreach (var question in quiz.Questions)
                {
                    var questionDto = quizDto.Questions[quiz.Questions.ToList().IndexOf(question)];
                    question.CorrectChoiceId = question.Choices.ToList()[questionDto.CorrectChoiceIndex].Id;
                }

                await _unitOfWork.Complete();

                return new ApiResponse(200, "تم إنشاء الاختبار بنجاح");
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, $"حدث خطأ أثناء إنشاء الاختبار: {ex.Message}");
            }
        }
        public async Task<ApiDataResponse> SubmitQuizAsync(string userId, QuizSubmissionDto submission)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || submission == null)
                    return new ApiDataResponse(400, null, "Invalid user ID or submission data");

                var duplicateQuestions = submission.StudentAnswers
                    .GroupBy(a => a.QuestionId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateQuestions.Any())
                    return new ApiDataResponse(400, null,
                        $"Duplicate answers found for question(s): {string.Join(", ", duplicateQuestions)}");

                var quiz = await _unitOfWork.Repository<Quiz>()
                    .GetEntityWithSpecAsync(new QuizWithQuestionsAndChoicesSpecification(submission.QuizId));

                var courseProgress = await _videoProgressService.CheckCourseCompletionEligibilityAsync(userId, quiz.CourseId);
                if (!courseProgress == true)
                    return new ApiDataResponse(400, null, "You are not eligible to take this quiz");

                if (quiz == null)
                    return new ApiDataResponse(404, null, $"Quiz with ID {submission.QuizId} not found");

                if (!quiz.Questions.Any())
                    return new ApiDataResponse(400, null, $"Quiz with ID {submission.QuizId} has no questions");

                var questionIds = quiz.Questions.Select(q => q.Id).ToHashSet();
                foreach (var answer in submission.StudentAnswers)
                {
                    if (!questionIds.Contains(answer.QuestionId))
                        return new ApiDataResponse(400, null, $"Question with ID {answer.QuestionId} not found in quiz");
                }

                var existingProgress = await _unitOfWork.Repository<Progress>()
                    .GetEntityWithSpecAsync(new ProgressByQuizAndUserSpecification(submission.QuizId, userId));

                if (existingProgress?.IsCompleted == true)
                    return new ApiDataResponse(400, null, $"Quiz already completed by user {userId}");

                var answerMap = submission.StudentAnswers
                    .DistinctBy(a => a.QuestionId)
                    .ToDictionary(a => a.QuestionId);

                var questionResults = new List<QuestionResultDto>();
                int correctAnswers = 0;

                foreach (var question in quiz.Questions)
                {
                    bool hasAnswer = answerMap.TryGetValue(question.Id, out var answer);
                    bool isCorrect = hasAnswer && question.CorrectChoiceId == answer!.SelectedChoice;

                    if (isCorrect) correctAnswers++;

                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        IsCorrect = isCorrect,
                        SelectedChoice = hasAnswer ? answer!.SelectedChoice : 0,
                        CorrectChoice = question.CorrectChoiceId
                    });
                }

                int totalQuestions = quiz.Questions.Count;
                int score = totalQuestions > 0 ? (correctAnswers * 100) / totalQuestions : 0;
                int unansweredCount = totalQuestions - submission.StudentAnswers.Count;
                bool isCompleted = score >= 60;

                Progress progress;
                if (existingProgress != null)
                {
                    existingProgress.Score = Math.Max(existingProgress.Score, score);
                    existingProgress.IsCompleted = existingProgress.IsCompleted || isCompleted;
                    existingProgress.CreatedAt = DateTime.UtcNow;
                    progress = existingProgress;
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
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    if (existingProgress != null)
                        _unitOfWork.Repository<Progress>().Update(progress);
                    else
                        await _unitOfWork.Repository<Progress>().AddAsync(progress);

                    await _unitOfWork.Complete();

                    if (isCompleted && (!existingProgress?.IsCompleted ?? true))
                    {
                        var certificate = await _certificateService.GenerateAndSaveCertificate(userId, submission.QuizId);

                        var notificationServiceDto = new NotificationServiceDto
                        {
                            Title = "مبارك على اجتيازك!",
                            Description = "الحمد لله، لقد اجتزت الامتحان بنجاح! نسأل الله أن يبارك لك في علمك وعملك، وأن يجعلك من النافعين لدينك وأمتك. يمكنك الآن استلام شهادتك من قسم الشهادات. نسأل الله لك التوفيق والسداد في مسيرتك العلمية.",
                            Type = NotificationType.Certificate,
                            UserId = userId
                        };

                        await _notificationService.SendNotificationAsync(notificationServiceDto);
                        await _unitOfWork.Repository<Certificate>().AddAsync((Certificate)certificate.Data);
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

                    return new ApiDataResponse(200, result, "Quiz submitted successfully");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiDataResponse(500, null, $"Failed to submit quiz: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return new ApiDataResponse(500, null, $"An unexpected error occurred: {ex.Message}");
            }
        }

     }
}