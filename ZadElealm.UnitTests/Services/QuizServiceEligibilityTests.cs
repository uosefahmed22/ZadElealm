using Xunit;
using Moq;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Service.AppServices;

namespace ZadElealm.UnitTests.Services
{
    public class QuizServiceEligibilityTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ICertificateService> _certificateService = new();
        private readonly Mock<IVideoProgressService> _videoProgressService = new();
        private readonly Mock<INotificationService> _notificationService = new();
        private readonly Mock<IGenericRepository<Quiz>> _quizRepository = new();
        private readonly Mock<IGenericRepository<Progress>> _progressRepository = new();
        private readonly Mock<IGenericRepository<Certificate>> _certificateRepository = new();

        private const string UserId = "user-1";
        private const int CourseId = 5;

        private QuizService CreateService()
        {
            _unitOfWork.Setup(u => u.Repository<Quiz>()).Returns(_quizRepository.Object);
            _unitOfWork.Setup(u => u.Repository<Progress>()).Returns(_progressRepository.Object);
            _unitOfWork.Setup(u => u.Repository<Certificate>()).Returns(_certificateRepository.Object);
            return new QuizService(_unitOfWork.Object, _certificateService.Object,
                _videoProgressService.Object, _notificationService.Object);
        }

        private static QuizSubmissionDto BuildSubmission()
            => new QuizSubmissionDto
            {
                QuizId = 1,
                StudentAnswers = new List<StudentAnswerDto>
                {
                    new StudentAnswerDto { QuestionId = 10, ChoiceId = 100 }
                }
            };

        [Fact]
        public async Task SubmitQuiz_WhenDuplicateAnswers_Returns400()
        {
            var submission = BuildSubmission();
            submission.StudentAnswers.Add(new StudentAnswerDto { QuestionId = 10, ChoiceId = 101 });

            var result = await CreateService().SubmitQuizAsync(UserId, submission);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task SubmitQuiz_WhenQuizNotFound_Returns404()
        {
            _quizRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync((Quiz)null!);

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task SubmitQuiz_WhenUserHasNotCompleted80Percent_Returns403_AndDoesNotSaveProgress()
        {
            var quiz = new Quiz { Id = 1, CourseId = CourseId };
            quiz.Questions = new List<Question>
            {
                new Question { Id = 10 }
            };

            _quizRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync(quiz);
            _progressRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Progress>>()))
                .ReturnsAsync((Progress)null!);
            _videoProgressService.Setup(v => v.CheckCourseCompletionEligibilityAsync(UserId, CourseId))
                .ReturnsAsync(false);

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(403, result.StatusCode);
            _unitOfWork.Verify(u => u.Complete(), Times.Never);
            _unitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenEligible_PersistsProgressInsideTransaction()
        {
            var quiz = new Quiz { Id = 1, CourseId = CourseId, Name = "Quiz" };
            quiz.Questions = new List<Question>
            {
                new Question { Id = 10, CorrectChoiceId = 100 }
            };

            _quizRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync(quiz);
            _progressRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Progress>>()))
                .ReturnsAsync((Progress)null!);
            _videoProgressService.Setup(v => v.CheckCourseCompletionEligibilityAsync(UserId, CourseId))
                .ReturnsAsync(true);
            _certificateService.Setup(c => c.GenerateAndSaveCertificate(UserId, 1))
                .ReturnsAsync(new ApiDataResponse(200, new Certificate()));
            _notificationService.Setup(n => n.SendNotificationAsync(It.IsAny<NotificationServiceDto>()))
                .ReturnsAsync(new ApiDataResponse(200, null, "ok"));

            Progress? savedProgress = null;
            _progressRepository
                .Setup(r => r.AddAsync(It.IsAny<Progress>()))
                .Callback<Progress>(p =>
                {
                    p.Id = 99;
                    p.CreatedAt = DateTime.UtcNow;
                    savedProgress = p;
                })
                .Returns(Task.CompletedTask);

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(savedProgress);
            Assert.True(savedProgress!.IsCompleted);
            _unitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task SubmitQuiz_WhenScoreIsBelowPassMark_DoesNotGenerateCertificate()
        {
            var quiz = new Quiz { Id = 1, CourseId = CourseId, Name = "Quiz" };
            quiz.Questions = new List<Question>
            {
                new Question { Id = 10, CorrectChoiceId = 999 },
                new Question { Id = 11, CorrectChoiceId = 999 },
                new Question { Id = 12, CorrectChoiceId = 999 }
            };

            _quizRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync(quiz);
            _progressRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Progress>>()))
                .ReturnsAsync((Progress)null!);
            _videoProgressService.Setup(v => v.CheckCourseCompletionEligibilityAsync(UserId, CourseId))
                .ReturnsAsync(true);

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(200, result.StatusCode);
            _certificateService.Verify(c => c.GenerateAndSaveCertificate(UserId, 1), Times.Never);
        }
    }
}
