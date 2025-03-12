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
            try
            {
                var quiz = await _unitOfWork.Repository<Quiz>()
                    .GetEntityWithSpecAsync(new QuizWithQuestionsAndChoicesSpecification(submission.QuizId));

                if (quiz == null)
                    throw new Exception("Quiz not found");

                var existingProgress = await _unitOfWork.Repository<Progress>()
                    .GetEntityWithSpecAsync(new ProgressByQuizAndUserSpecification(submission.QuizId, userId));

                if (existingProgress != null && existingProgress.IsCompleted)
                    throw new Exception("Quiz already completed");

                int correctAnswers = 0;
                foreach (var answer in submission.StudentAnswers)
                {
                    var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question != null && question.CorrectChoice == answer.StudentChoice)
                    {
                        correctAnswers++;
                    }
                }

                int totalQuestions = quiz.Questions.Count;
                if (totalQuestions == 0)
                    throw new Exception("No questions found in this quiz");

                int score = (correctAnswers * 100) / totalQuestions;
                bool isCompleted = score >= 60;

                var progress = new Progress
                {
                    QuizId = submission.QuizId,
                    UserId = userId,
                    Score = score,
                    IsCompleted = isCompleted,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Repository<Progress>().AddAsync(progress);
                await _unitOfWork.Complete();

                if (isCompleted)
                {
                    await _notificationService.SendNotificationAsync(new NotificationServiceDto
                    {
                        UserId = userId,
                        Type = NotificationType.Certificate,
                        Title = "مبارك على اجتيازك!",
                        Description = "الحمد لله، لقد اجتزت الامتحان بنجاح! نسأل الله أن يبارك لك في علمك وعملك، وأن يجعلك من النافعين لدينك وأمتك. يمكنك الآن استلام شهادتك من قسم الشهادات. نسأل الله لك التوفيق والسداد في مسيرتك العلمية."
                    });
                }

                return new QuizResultDto
                {
                    Score = score,
                    IsCompleted = isCompleted,
                    Date = progress.CreatedAt,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswers
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
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
    }
}