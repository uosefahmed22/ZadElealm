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
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Quiz;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Service.Errors;

namespace ZadElealm.Service.AppServices
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public QuizService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<ServiceApiResponse<QuizResultDto>> SubmitQuizAsync(string userId, QuizSubmissionDto submission)
        {
            if (submission == null)
                return ServiceApiResponse<QuizResultDto>.Failure("Quiz submission cannot be null", 400);

            if (string.IsNullOrEmpty(userId))
                return ServiceApiResponse<QuizResultDto>.Failure("User ID cannot be null or empty", 400);

            if (submission.QuizId <= 0)
                return ServiceApiResponse<QuizResultDto>.Failure("Invalid Quiz ID", 400);

            if (submission.StudentAnswers == null || !submission.StudentAnswers.Any())
                return ServiceApiResponse<QuizResultDto>.Failure("No answers provided", 400);

            try
            {
                var spec = new QuizWithQuestionsAndChoicesSpecification(submission.QuizId);
                var quiz = await _unitOfWork.Repository<Quiz>()
                    .GetEntityWithSpecAsync(spec);

                if (quiz == null)
                    return ServiceApiResponse<QuizResultDto>.Failure($"Quiz with ID {submission.QuizId} not found", 404);

                if (!quiz.Questions.Any())
                    return ServiceApiResponse<QuizResultDto>.Failure($"Quiz with ID {submission.QuizId} does not contain any questions", 400);

                var existingProgress = await _unitOfWork.Repository<Progress>()
                    .GetEntityWithSpecAsync(new ProgressByQuizAndUserSpecification(submission.QuizId, userId));

                if (existingProgress != null && existingProgress.IsCompleted)
                    return ServiceApiResponse<QuizResultDto>.Failure($"Quiz with ID {submission.QuizId} has already been completed by user {userId}", 400);

                var questionMap = quiz.Questions.ToDictionary(q => q.Id);
                foreach (var answer in submission.StudentAnswers)
                {
                    if (!questionMap.ContainsKey(answer.QuestionId))
                        return ServiceApiResponse<QuizResultDto>.Failure($"Question with ID {answer.QuestionId} not found in quiz", 400);
                }

                int totalQuestions = quiz.Questions.Count;
                int correctAnswers = 0;
                var questionResults = new List<QuestionResultDto>();

                var answeredQuestionIds = new HashSet<int>(submission.StudentAnswers.Select(a => a.QuestionId));
                var unansweredQuestions = quiz.Questions.Where(q => !answeredQuestionIds.Contains(q.Id)).ToList();

                foreach (var question in quiz.Questions)
                {
                    var answer = submission.StudentAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

                    bool isCorrect = false;
                    int selectedChoice = 0;

                    if (answer != null)
                    {
                        selectedChoice = answer.SelectedChoice;
                        isCorrect = question.CorrectChoice == selectedChoice;

                        if (isCorrect)
                        {
                            correctAnswers++;
                        }
                    }

                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        IsCorrect = isCorrect,
                        SelectedChoice = selectedChoice,
                        CorrectChoice = question.CorrectChoice
                    });
                }

                int score = totalQuestions > 0 ? (correctAnswers * 100) / totalQuestions : 0;
                bool isCompleted = score >= 60;

                Progress progress;
                if (existingProgress != null)
                {
                    progress = existingProgress;
                    progress.Score = Math.Max(progress.Score, score);
                    progress.IsCompleted = progress.IsCompleted || isCompleted;
                    progress.CreatedAt = DateTime.UtcNow;
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
                    {
                        _unitOfWork.Repository<Progress>().Update(progress);
                    }
                    else
                    {
                        await _unitOfWork.Repository<Progress>().AddAsync(progress);
                    }

                    if (isCompleted && (!existingProgress?.IsCompleted ?? true))
                    {
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

                        await _unitOfWork.Repository<Notification>().AddAsync(notification);
                    }

                    await _unitOfWork.Complete();
                    await _unitOfWork.CommitTransactionAsync();

                    var result = new QuizResultDto
                    {
                        QuizName = quiz.Name,
                        Score = score,
                        IsCompleted = isCompleted,
                        TotalQuestions = totalQuestions,
                        CorrectAnswers = correctAnswers,
                        UnansweredQuestions = unansweredQuestions.Count,
                        Date = progress.CreatedAt,
                        QuestionResults = questionResults
                    };

                    return ServiceApiResponse<QuizResultDto>.Success(result, "Quiz submitted successfully");
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ServiceApiResponse<QuizResultDto>.Failure("Failed to process quiz submission. Please try again later.", 500);
                }
            }
            catch (Exception)
            {
                return ServiceApiResponse<QuizResultDto>.Failure("An unexpected error occurred while processing the quiz", 500);
            }
        }
        public async Task<ServiceApiResponse<int>> CreateQuizAsync(QuizDto quizDto)
        {
            try
            {
                if (quizDto == null)
                    return ServiceApiResponse<int>.Failure("Quiz data cannot be null", 400);

                if (string.IsNullOrEmpty(quizDto.Name))
                    return ServiceApiResponse<int>.Failure("Quiz name is required", 400);

                if (!quizDto.Questions?.Any() ?? true)
                    return ServiceApiResponse<int>.Failure("Quiz must contain at least one question", 400);

                var quiz = new Quiz
                {
                    Name = quizDto.Name,
                    Description = quizDto.Description,
                    CourseId = quizDto.CourseId,
                    Questions = quizDto.Questions.Select(q => new Question
                    {
                        Text = q.Text,
                        CorrectChoice = q.CorrectChoice,
                        Choices = q.Choices.Select(c => new Choice
                        {
                            Text = c.Text
                        }).ToList()
                    }).ToList()
                };

                await _unitOfWork.Repository<Quiz>().AddAsync(quiz);
                await _unitOfWork.Complete();

                return ServiceApiResponse<int>.Success(quiz.Id, "Quiz created successfully");
            }
            catch (Exception ex)
            {
                return ServiceApiResponse<int>.Failure("Failed to create quiz", 500);
            }
        }
        public async Task<ServiceApiResponse<bool>> UpdateQuizAsync(QuizDto quizDto)
        {
            try
            {
                if (quizDto == null)
                    return ServiceApiResponse<bool>.Failure("Quiz data cannot be null", 400);

                if (quizDto.Id <= 0)
                    return ServiceApiResponse<bool>.Failure("Invalid quiz ID", 400);

                var spec = new QuizWithQuestionsAndChoicesSpecification(quizDto.Id);
                var quiz = await _unitOfWork.Repository<Quiz>().GetEntityWithSpecAsync(spec);

                if (quiz == null)
                    return ServiceApiResponse<bool>.Failure("Quiz not found", 404);

                if (!quizDto.Questions?.Any() ?? true)
                    return ServiceApiResponse<bool>.Failure("Quiz must contain at least one question", 400);

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    quiz.Name = quizDto.Name;
                    quiz.Description = quizDto.Description;
                    quiz.CourseId = quizDto.CourseId;

                    var questionIdsInDto = quizDto.Questions
                        .Where(q => q.Id > 0)
                        .Select(q => q.Id)
                        .ToList();

                    var questionsToRemove = quiz.Questions
                        .Where(q => !questionIdsInDto.Contains(q.Id))
                        .ToList();

                    foreach (var questionToRemove in questionsToRemove)
                    {
                        quiz.Questions.Remove(questionToRemove);
                    }

                    foreach (var questionDto in quizDto.Questions)
                    {
                        if (questionDto.Id > 0)
                        {
                            var existingQuestion = quiz.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                            if (existingQuestion != null)
                            {
                                existingQuestion.Text = questionDto.Text;
                                existingQuestion.CorrectChoice = questionDto.CorrectChoice;

                                existingQuestion.Choices.Clear();
                                existingQuestion.Choices = questionDto.Choices.Select(c => new Choice
                                {
                                    Text = c.Text
                                }).ToList();
                            }
                        }
                        else
                        {
                            quiz.Questions.Add(new Question
                            {
                                Text = questionDto.Text,
                                CorrectChoice = questionDto.CorrectChoice,
                                Choices = questionDto.Choices.Select(c => new Choice
                                {
                                    Text = c.Text
                                }).ToList()
                            });
                        }
                    }

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
    }
}