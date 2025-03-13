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

        public async Task<QuizResultDto> SubmitQuizAsync(string userId, QuizSubmissionDto submission)
        {
            if (submission == null)
                throw new ArgumentNullException(nameof(submission), "Quiz submission cannot be null");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");

            if (submission.QuizId <= 0)
                throw new ArgumentException("Invalid Quiz ID", nameof(submission.QuizId));

            if (submission.StudentAnswers == null || !submission.StudentAnswers.Any())
                throw new ArgumentException("No answers provided", nameof(submission.StudentAnswers));

            try
            {
                var spec = new QuizWithQuestionsAndChoicesSpecification(submission.QuizId);
                var quiz = await _unitOfWork.Repository<Quiz>()
                    .GetEntityWithSpecAsync(spec);

                if (quiz == null)
                    throw new Exception($"Quiz with ID {submission.QuizId} not found.");

                if (!quiz.Questions.Any())
                    throw new Exception($"Quiz with ID {submission.QuizId} does not contain any questions");

                var existingProgress = await _unitOfWork.Repository<Progress>()
                    .GetEntityWithSpecAsync(new ProgressByQuizAndUserSpecification(submission.QuizId, userId));

                if (existingProgress != null && existingProgress.IsCompleted)
                    throw new Exception($"Quiz with ID {submission.QuizId} has already been completed by user {userId}.");

                var questionMap = quiz.Questions.ToDictionary(q => q.Id);

                foreach (var answer in submission.StudentAnswers)
                {
                    if (!questionMap.ContainsKey(answer.QuestionId))
                        throw new Exception($"Question with ID {answer.QuestionId} not found in quiz.");
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
                        UserId = userId,
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
                            UserId = userId,
                            IsRead = false
                        }
                    }
                        };

                        await _unitOfWork.Repository<Notification>().AddAsync(notification);
                    }

                    await _unitOfWork.Complete();
                    await _unitOfWork.CommitTransactionAsync();

                    return new QuizResultDto
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
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ApplicationException("Failed to process quiz submission. Please try again later.", ex);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to process quiz submission. Please try again later.", ex);
            }
        }
        public async Task CreateQuizAsync(QuizDto quizDto)
        {
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
        }
        public async Task UpdateQuizAsync(QuizDto quizDto)
        {
            if (quizDto == null)
            {
                throw new ArgumentNullException(nameof(quizDto));
            }

            var spec = new QuizWithQuestionsAndChoicesSpecification(quizDto.Id);
            var quiz = await _unitOfWork.Repository<Quiz>().GetEntityWithSpecAsync(spec);

            if (quiz == null)
            {
                throw new Exception("الامتحان غير موجود.");
            }

            // Update basic quiz properties
            quiz.Name = quizDto.Name;
            quiz.Description = quizDto.Description;
            quiz.CourseId = quizDto.CourseId;

            // Create a list of question IDs from the DTO to track which ones to keep
            var questionIdsInDto = quizDto.Questions
                .Where(q => q.Id > 0)
                .Select(q => q.Id)
                .ToList();

            // Find questions to remove (those in the database but not in the DTO)
            var questionsToRemove = quiz.Questions
                .Where(q => !questionIdsInDto.Contains(q.Id))
                .ToList();

            // Remove questions that were deleted in the UI
            foreach (var questionToRemove in questionsToRemove)
            {
                quiz.Questions.Remove(questionToRemove);
            }

            // Update existing questions and add new ones
            for (int i = 0; i < quizDto.Questions.Count; i++)
            {
                var questionDto = quizDto.Questions[i];
                if (questionDto.Id > 0)
                {
                    var existingQuestion = quiz.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                    if (existingQuestion != null)
                    {
                        existingQuestion.Text = questionDto.Text;
                        existingQuestion.CorrectChoice = questionDto.CorrectChoice;

                        // Clear and update choices
                        existingQuestion.Choices.Clear();
                        existingQuestion.Choices = questionDto.Choices.Select(c => new Choice
                        {
                            Text = c.Text
                        }).ToList();
                    }
                }
                else
                {
                    // Add new question
                    var newQuestion = new Question
                    {
                        Text = questionDto.Text,
                        CorrectChoice = questionDto.CorrectChoice,
                        Choices = questionDto.Choices.Select(c => new Choice
                        {
                            Text = c.Text
                        }).ToList()
                    };
                    quiz.Questions.Add(newQuestion);
                }
            }

            await _unitOfWork.Complete();
        }
    }
}