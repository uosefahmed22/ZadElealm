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

namespace ZadElealm.Service.AppServices
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public QuizService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<QuizResultDto> SubmitQuizAsync(string userId, QuizSubmissionDto submission)
        {
            try
            {
                var quiz = await _unitOfWork.Repository<Quiz>()
                    .GetEntityWithSpecAsync(new QuizWithQuestionsAndChoicesSpecification(submission.QuizId));

                if (quiz == null)
                    throw new Exception("الامتحان غير موجود");

                var existingProgress = await _unitOfWork.Repository<Progress>()
                    .GetEntityWithSpecAsync(new ProgressByQuizAndUserSpecification(submission.QuizId, userId));

                if (existingProgress != null && existingProgress.IsCompleted)
                    throw new Exception("تم إجراء الامتحان مسبقاً");

                int correctAnswers = 0;
                foreach (var answer in submission.Answers)
                {
                    var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question != null && question.CorrectChoice == answer.SelectedChoice)
                    {
                        correctAnswers++;
                    }
                }

                int totalQuestions = quiz.Questions.Count;
                if (totalQuestions == 0)
                    throw new Exception("لا توجد أسئلة في هذا الامتحان");

                int score = (correctAnswers * 100) / totalQuestions;
                bool isCompleted = score >= 60;

                var progress = new Progress
                {
                    QuizId = submission.QuizId,
                    UserId = userId,
                    Score = score,
                    IsCompleted = isCompleted,
                    Date = DateTime.Now
                };

                await _unitOfWork.Repository<Progress>().AddAsync(progress);
                await _unitOfWork.Complete();

                return new QuizResultDto
                {
                    Score = score,
                    IsCompleted = isCompleted,
                    Date = progress.Date,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswers
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}