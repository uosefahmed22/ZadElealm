using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Quiz;
using ZadElealm.Core.Enums;
using ZadElealm.Service.Errors;
using ZadElealm.Core.ServiceDto;
using static ZadElealm.Service.AppServices.QuizService;

namespace ZadElealm.Service.AppServices
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICertificateService _certificateService;
        private readonly INotificationService _notificationService;

        public QuizService(IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            ICertificateService certificateService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _certificateService = certificateService;
            _notificationService = notificationService;
        }

        public async Task<ServiceApiResponse<QuizResultDto>> SubmitQuizAsync(string userId, QuizSubmissionDto submission)
        {
            var (isValid, error) = ValidateSubmission(userId, submission);
            if (!isValid)
                return ServiceApiResponse<QuizResultDto>.Failure(error, 400);

            try
            {
                var spec = new QuizWithQuestionsAndChoicesSpecification(submission.QuizId);
                var quiz = await _unitOfWork.Repository<Quiz>().GetEntityWithSpecAsync(spec);

                if (quiz == null)
                    return ServiceApiResponse<QuizResultDto>.Failure($"Quiz with ID {submission.QuizId} not found", 404);

                if (!quiz.Questions.Any())
                    return ServiceApiResponse<QuizResultDto>.Failure($"Quiz with ID {submission.QuizId} does not contain any questions", 400);

                var questionMap = quiz.Questions.ToDictionary(q => q.Id);
                foreach (var answer in submission.StudentAnswers)
                {
                    if (!questionMap.ContainsKey(answer.QuestionId))
                        return ServiceApiResponse<QuizResultDto>.Failure($"Question with ID {answer.QuestionId} not found in quiz", 400);
                }

                var existingProgress = await _unitOfWork.Repository<Progress>()
                    .GetEntityWithSpecAsync(new ProgressByQuizAndUserSpecification(submission.QuizId, userId));

                if (existingProgress?.IsCompleted == true)
                    return ServiceApiResponse<QuizResultDto>.Failure($"Quiz with ID {submission.QuizId} has already been completed by user {userId}", 400);

                var (score, correctAnswers, questionResults, unansweredCount) =
                    ProcessQuizAnswers(quiz, submission.StudentAnswers);

                bool isCompleted = score >= 60;

                var progress = PrepareProgressEntity(existingProgress, userId, submission.QuizId, score, isCompleted);

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    if (existingProgress != null)
                    {
                        _unitOfWork.Repository<Progress>().Update(progress);
                    }
                    else
                    {
                        await _unitOfWork.Repository<Progress>().AddAsync(progress);
                    }

                    await _unitOfWork.Complete();

                    if (isCompleted && (!existingProgress?.IsCompleted ?? true))
                    {
                        var certificate = await _certificateService.GenerateAndSaveCertificate(userId, submission.QuizId);

                        var notification = new Notification
                        {
                            Title = "مبارك على اجتيازك!",
                            Description = "الحمد لله، لقد اجتزت الامتحان بنجاح! نسأل الله أن يبارك لك في علمك وعملك، وأن يجعلك من النافعين لدينك وأمتك. يمكنك الآن استلام شهادتك من قسم الشهادات. نسأل الله لك التوفيق والسداد في مسيرتك العلمية.",
                            Type = NotificationType.Certificate,
                            UserNotifications = new List<UserNotification>
                        {
                            new UserNotification
                            {
                                AppUserId = userId,
                                IsRead = false
                            }
                        }
                        };

                        await _unitOfWork.Repository<Certificate>().AddAsync(certificate);
                        await _unitOfWork.Repository<Notification>().AddAsync(notification);
                        await _unitOfWork.Complete();
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    var result = new QuizResultDto
                    {
                        QuizName = quiz.Name,
                        Score = score,
                        IsCompleted = isCompleted,
                        TotalQuestions = quiz.Questions.Count,
                        CorrectAnswers = correctAnswers,
                        UnansweredQuestions = unansweredCount,
                        Date = progress.CreatedAt,
                        QuestionResults = questionResults
                    };

                    return ServiceApiResponse<QuizResultDto>.Success(result, "Quiz submitted successfully");
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ServiceApiResponse<QuizResultDto>.Failure("Failed to process quiz submission", 500);
                }
            }
            catch (Exception)
            {
                return ServiceApiResponse<QuizResultDto>.Failure("An unexpected error occurred", 500);
            }
        }
        private (bool IsValid, string Error) ValidateSubmission(string userId, QuizSubmissionDto submission)
        {
            if (submission == null)
                return (false, "Quiz submission cannot be null");

            if (string.IsNullOrEmpty(userId))
                return (false, "User ID cannot be null or empty");

            if (submission.QuizId <= 0)
                return (false, "Invalid Quiz ID");

            if (submission.StudentAnswers == null || !submission.StudentAnswers.Any())
                return (false, "No answers provided");

            return (true, null);
        }
        private (int Score, int CorrectAnswers, List<QuestionResultDto> QuestionResults, int UnansweredCount) ProcessQuizAnswers(Quiz quiz, List<StudentAnswerDto> studentAnswers)
        {
            var answerMap = studentAnswers.ToDictionary(a => a.QuestionId);
            var questionResults = new List<QuestionResultDto>();
            int correctAnswers = 0;

            foreach (var question in quiz.Questions)
            {
                if (answerMap.TryGetValue(question.Id, out var answer))
                {
                    bool isCorrect = question.CorrectChoice == answer.SelectedChoice;
                    if (isCorrect) correctAnswers++;

                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        IsCorrect = isCorrect,
                        SelectedChoice = answer.SelectedChoice,
                        CorrectChoice = question.CorrectChoice
                    });
                }
                else
                {
                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        IsCorrect = false,
                        SelectedChoice = 0,
                        CorrectChoice = question.CorrectChoice
                    });
                }
            }

            int totalQuestions = quiz.Questions.Count;
            int score = totalQuestions > 0 ? (correctAnswers * 100) / totalQuestions : 0;
            int unansweredCount = totalQuestions - studentAnswers.Count;

            return (score, correctAnswers, questionResults, unansweredCount);
        }
        private Progress PrepareProgressEntity(Progress existingProgress, string userId, int quizId, int score, bool isCompleted)
        {
            if (existingProgress != null)
            {
                existingProgress.Score = Math.Max(existingProgress.Score, score);
                existingProgress.IsCompleted = existingProgress.IsCompleted || isCompleted;
                existingProgress.CreatedAt = DateTime.UtcNow;
                return existingProgress;
            }

            return new Progress
            {
                QuizId = quizId,
                AppUserId = userId,
                Score = score,
                IsCompleted = isCompleted,
                CreatedAt = DateTime.UtcNow
            };
        }
       
        public async Task<ServiceApiResponse<int>> CreateQuizAsync(QuizDto quizDto)
        {
            var (isValid, error) = ValidateQuizDto(quizDto);
            if (!isValid)
                return ServiceApiResponse<int>.Failure(error, 400);

            try
            {
                var quiz = new Quiz
                {
                    Name = quizDto.Name.Trim(),
                    Description = quizDto.Description?.Trim(),
                    CourseId = quizDto.CourseId,
                    Questions = quizDto.Questions.Select(q => new Question
                    {
                        Text = q.Text.Trim(),
                        CorrectChoice = q.CorrectChoice,
                        Choices = q.Choices.Select(c => new Choice
                        {
                            Text = c.Text.Trim()
                        }).ToList()
                    }).ToList()
                };

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Repository<Quiz>().AddAsync(quiz);
                    await _unitOfWork.Complete();
                    await _unitOfWork.CommitTransactionAsync();

                    return ServiceApiResponse<int>.Success(quiz.Id, "Quiz created successfully");
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ServiceApiResponse<int>.Failure("Failed to save quiz", 500);
                }
            }
            catch (Exception)
            {
                return ServiceApiResponse<int>.Failure("An unexpected error occurred while creating the quiz", 500);
            }
        }
       
        public async Task<ServiceApiResponse<bool>> UpdateQuizAsync(QuizDto quizDto)
        {
            var (isValid, error) = ValidateQuizDto(quizDto);
            if (!isValid)
                return ServiceApiResponse<bool>.Failure(error, 400);

            try
            {
                var spec = new QuizWithQuestionsAndChoicesSpecification(quizDto.Id);
                var quiz = await _unitOfWork.Repository<Quiz>().GetEntityWithSpecAsync(spec);

                if (quiz == null)
                    return ServiceApiResponse<bool>.Failure("Quiz not found", 404);

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    UpdateQuizBasicInfo(quiz, quizDto);
                    await UpdateQuizQuestions(quiz, quizDto.Questions);

                    await _unitOfWork.Complete();
                    await _unitOfWork.CommitTransactionAsync();

                    return ServiceApiResponse<bool>.Success(true, "Quiz updated successfully");
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ServiceApiResponse<bool>.Failure("Failed to update quiz", 500);
                }
            }
            catch (Exception)
            {
                return ServiceApiResponse<bool>.Failure("An unexpected error occurred", 500);
            }
        }

        private (bool IsValid, string Error) ValidateQuizDto(QuizDto quizDto)
        {
            if (quizDto == null)
                return (false, "Quiz data cannot be null");

            if (quizDto.Id <= 0)
                return (false, "Invalid quiz ID");

            if (string.IsNullOrWhiteSpace(quizDto.Name))
                return (false, "Quiz name is required");

            if (!quizDto.Questions?.Any() ?? true)
                return (false, "Quiz must contain at least one question");

            foreach (var question in quizDto.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                    return (false, "Question text cannot be empty");

                if (question.Choices == null || question.Choices.Count < 2)
                    return (false, "Each question must have at least 2 choices");

                if (question.CorrectChoice <= 0 || question.CorrectChoice > question.Choices.Count)
                    return (false, "Invalid correct choice number");

                if (question.Choices.Any(c => string.IsNullOrWhiteSpace(c.Text)))
                    return (false, "Choice text cannot be empty");
            }

            return (true, null);
        }
        private void UpdateQuizBasicInfo(Quiz quiz, QuizDto quizDto)
        {
            quiz.Name = quizDto.Name.Trim();
            quiz.Description = quizDto.Description?.Trim();
            quiz.CourseId = quizDto.CourseId;
        }
        private async Task UpdateQuizQuestions(Quiz quiz, List<QuestionDto> questionDtos)
        {
            var questionIdsToKeep = questionDtos
                .Where(q => q.Id > 0)
                .Select(q => q.Id)
                .ToHashSet();

            var questionsToRemove = quiz.Questions
                .Where(q => !questionIdsToKeep.Contains(q.Id))
                .ToList();

            foreach (var question in questionsToRemove)
            {
                quiz.Questions.Remove(question);
            }

            foreach (var questionDto in questionDtos)
            {
                if (questionDto.Id > 0)
                {
                    UpdateExistingQuestion(quiz, questionDto);
                }
                else
                {
                    AddNewQuestion(quiz, questionDto);
                }
            }
        }
        private void UpdateExistingQuestion(Quiz quiz, QuestionDto questionDto)
        {
            var existingQuestion = quiz.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
            if (existingQuestion != null)
            {
                existingQuestion.Text = questionDto.Text.Trim();
                existingQuestion.CorrectChoice = questionDto.CorrectChoice;

                existingQuestion.Choices.Clear();
                existingQuestion.Choices = questionDto.Choices
                    .Select(c => new Choice { Text = c.Text.Trim() })
                    .ToList();
            }
        }
        private void AddNewQuestion(Quiz quiz, QuestionDto questionDto)
        {
            quiz.Questions.Add(new Question
            {
                Text = questionDto.Text.Trim(),
                CorrectChoice = questionDto.CorrectChoice,
                Choices = questionDto.Choices
                    .Select(c => new Choice { Text = c.Text.Trim() })
                    .ToList()
            });
        }
    }
}