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

        private static Question BuildQuestion(int id, int correctChoiceId = 100)
            => new()
            {
                Id = id,
                Text = $"Question {id}",
                CorrectChoiceId = correctChoiceId,
                Choices = new List<Choice>
                {
                    new() { Id = 100, Text = "A" },
                    new() { Id = 101, Text = "B" }
                }
            };

        private void SetupSubmissionDependencies(Quiz quiz, Progress? existingProgress = null)
        {
            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync(quiz);
            _progressRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Progress>>()))
                .ReturnsAsync(existingProgress!);
            _videoProgressService.Setup(v => v.CheckCourseCompletionEligibilityAsync(UserId, quiz.CourseId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task SubmitQuiz_WhenDuplicateAnswers_Returns400()
        {
            var submission = BuildSubmission();
            submission.StudentAnswers.Add(new StudentAnswerDto { QuestionId = 10, ChoiceId = 101 });

            var result = await CreateService().SubmitQuizAsync(UserId, submission);

            Assert.Equal(400, result.StatusCode);
            _quizRepository.Verify(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenAnswerIdentifiersAreInvalid_Returns400BeforeDatabaseAccess()
        {
            var submission = BuildSubmission();
            submission.StudentAnswers[0].ChoiceId = 0;

            var result = await CreateService().SubmitQuizAsync(UserId, submission);

            Assert.Equal(400, result.StatusCode);
            _quizRepository.Verify(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenQuizNotFound_Returns404()
        {
            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()))
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
                BuildQuestion(10)
            };

            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()))
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
        public async Task SubmitQuiz_WhenChoiceDoesNotBelongToQuestion_Returns400BeforeProgressLookup()
        {
            var quiz = new Quiz
            {
                Id = 1,
                CourseId = CourseId,
                Questions = new List<Question> { BuildQuestion(10) }
            };
            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync(quiz);
            var submission = BuildSubmission();
            submission.StudentAnswers[0].ChoiceId = 999;

            var result = await CreateService().SubmitQuizAsync(UserId, submission);

            Assert.Equal(400, result.StatusCode);
            _progressRepository.Verify(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Progress>>()), Times.Never);
            _videoProgressService.Verify(v => v.CheckCourseCompletionEligibilityAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenAlreadyCompleted_Returns400WithoutEligibilityCheckOrTransaction()
        {
            var quiz = new Quiz
            {
                Id = 1,
                CourseId = CourseId,
                Questions = new List<Question> { BuildQuestion(10) }
            };
            var progress = new Progress { IsCompleted = true, Score = 100 };
            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync(quiz);
            _progressRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Progress>>()))
                .ReturnsAsync(progress);

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(400, result.StatusCode);
            _videoProgressService.Verify(v => v.CheckCourseCompletionEligibilityAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _unitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenEligible_PersistsProgressInsideTransaction()
        {
            var quiz = new Quiz { Id = 1, CourseId = CourseId, Name = "Quiz" };
            quiz.Questions = new List<Question>
            {
                BuildQuestion(10)
            };

            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()))
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
            _unitOfWork.Verify(u => u.Complete(), Times.Exactly(2));
            _quizRepository.Verify(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()), Times.Once);
            _quizRepository.Verify(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Quiz>>()), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenScoreIsBelowPassMark_DoesNotGenerateCertificate()
        {
            var quiz = new Quiz { Id = 1, CourseId = CourseId, Name = "Quiz" };
            quiz.Questions = new List<Question>
            {
                BuildQuestion(10, 999),
                BuildQuestion(11, 999),
                BuildQuestion(12, 999)
            };

            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync(quiz);
            _progressRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Progress>>()))
                .ReturnsAsync((Progress)null!);
            _videoProgressService.Setup(v => v.CheckCourseCompletionEligibilityAsync(UserId, CourseId))
                .ReturnsAsync(true);

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(200, result.StatusCode);
            _certificateService.Verify(c => c.GenerateAndSaveCertificate(UserId, 1), Times.Never);
            _notificationService.Verify(n => n.SendNotificationAsync(It.IsAny<NotificationServiceDto>()), Times.Never);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task SubmitQuiz_WhenPreviousAttemptFailedAndRetryPasses_GeneratesCertificate()
        {
            var quiz = new Quiz
            {
                Id = 1,
                CourseId = CourseId,
                Name = "Quiz",
                Questions = new List<Question> { BuildQuestion(10) }
            };
            var existingProgress = new Progress
            {
                Id = 7,
                AppUserId = UserId,
                QuizId = 1,
                Score = 20,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(It.IsAny<ISpecification<Quiz>>()))
                .ReturnsAsync(quiz);
            _progressRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Progress>>()))
                .ReturnsAsync(existingProgress);
            _videoProgressService.Setup(v => v.CheckCourseCompletionEligibilityAsync(UserId, CourseId))
                .ReturnsAsync(true);
            _certificateService.Setup(c => c.GenerateAndSaveCertificate(UserId, 1))
                .ReturnsAsync(new ApiDataResponse(200, new Certificate()));
            _notificationService.Setup(n => n.SendNotificationAsync(It.IsAny<NotificationServiceDto>()))
                .ReturnsAsync(new ApiDataResponse(200, null, "ok"));

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(200, result.StatusCode);
            Assert.True(existingProgress.IsCompleted);
            Assert.Equal(100, existingProgress.Score);
            _certificateService.Verify(c => c.GenerateAndSaveCertificate(UserId, 1), Times.Once);
        }

        [Fact]
        public async Task SubmitQuiz_WhenCertificateGenerationFails_RollsBackWithoutNotificationOrCommit()
        {
            var quiz = new Quiz
            {
                Id = 1,
                CourseId = CourseId,
                Name = "Quiz",
                Questions = new List<Question> { BuildQuestion(10) }
            };
            SetupSubmissionDependencies(quiz);
            _certificateService.Setup(c => c.GenerateAndSaveCertificate(UserId, 1))
                .ReturnsAsync(new ApiDataResponse(500, null, "failed"));

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(500, result.StatusCode);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _notificationService.Verify(n => n.SendNotificationAsync(It.IsAny<NotificationServiceDto>()), Times.Never);
            _certificateRepository.Verify(r => r.AddAsync(It.IsAny<Certificate>()), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenCertificateResponseHasNoCertificate_RollsBack()
        {
            var quiz = new Quiz
            {
                Id = 1,
                CourseId = CourseId,
                Name = "Quiz",
                Questions = new List<Question> { BuildQuestion(10) }
            };
            SetupSubmissionDependencies(quiz);
            _certificateService.Setup(c => c.GenerateAndSaveCertificate(UserId, 1))
                .ReturnsAsync(new ApiDataResponse(200, null, "invalid payload"));

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(500, result.StatusCode);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _notificationService.Verify(n => n.SendNotificationAsync(It.IsAny<NotificationServiceDto>()), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenNotificationFails_RollsBackWithoutCertificateInsertOrCommit()
        {
            var quiz = new Quiz
            {
                Id = 1,
                CourseId = CourseId,
                Name = "Quiz",
                Questions = new List<Question> { BuildQuestion(10) }
            };
            SetupSubmissionDependencies(quiz);
            _certificateService.Setup(c => c.GenerateAndSaveCertificate(UserId, 1))
                .ReturnsAsync(new ApiDataResponse(200, new Certificate()));
            _notificationService.Setup(n => n.SendNotificationAsync(It.IsAny<NotificationServiceDto>()))
                .ReturnsAsync(new ApiDataResponse(500, null, "failed"));

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(500, result.StatusCode);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _certificateRepository.Verify(r => r.AddAsync(It.IsAny<Certificate>()), Times.Never);
        }

        [Fact]
        public async Task SubmitQuiz_WhenSavingProgressThrows_RollsBack()
        {
            var quiz = new Quiz
            {
                Id = 1,
                CourseId = CourseId,
                Name = "Quiz",
                Questions = new List<Question> { BuildQuestion(10, 999) }
            };
            SetupSubmissionDependencies(quiz);
            _unitOfWork.Setup(u => u.Complete()).ThrowsAsync(new InvalidOperationException("save failed"));

            var result = await CreateService().SubmitQuizAsync(UserId, BuildSubmission());

            Assert.Equal(500, result.StatusCode);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateQuiz_WithTwoQuestions_SavesOnlyTwice()
        {
            Quiz? addedQuiz = null;
            _quizRepository.Setup(r => r.AddAsync(It.IsAny<Quiz>()))
                .Callback<Quiz>(quiz => addedQuiz = quiz)
                .Returns(Task.CompletedTask);
            _unitOfWork.SetupSequence(u => u.Complete())
                .ReturnsAsync(() =>
                {
                    var nextId = 10;
                    foreach (var question in addedQuiz!.Questions)
                    {
                        question.Id = nextId++;
                        foreach (var choice in question.Choices)
                            choice.Id = nextId++;
                    }
                    return 1;
                })
                .ReturnsAsync(1);

            var dto = new CreateQuizDto
            {
                CourseId = CourseId,
                Name = "Quiz",
                Description = "Description",
                Questions = new List<CreateQuestionDto>
                {
                    new()
                    {
                        Text = "Q1",
                        CorrectChoiceIndex = 1,
                        Choices = new List<CreateChoiceDto>
                        {
                            new() { Text = "A" }, new() { Text = "B" }
                        }
                    },
                    new()
                    {
                        Text = "Q2",
                        CorrectChoiceIndex = 0,
                        Choices = new List<CreateChoiceDto>
                        {
                            new() { Text = "A" }, new() { Text = "B" }
                        }
                    }
                }
            };

            var result = await CreateService().CreateQuizAsync(dto);

            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(addedQuiz);
            Assert.Equal(addedQuiz!.Questions[0].Choices[1].Id, addedQuiz.Questions[0].CorrectChoiceId);
            Assert.Equal(addedQuiz.Questions[1].Choices[0].Id, addedQuiz.Questions[1].CorrectChoiceId);
            _unitOfWork.Verify(u => u.Complete(), Times.Exactly(2));
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateQuiz_WhenCorrectChoiceIndexIsNegative_Returns400WithoutTransaction()
        {
            var dto = new CreateQuizDto
            {
                CourseId = CourseId,
                Name = "Quiz",
                Description = "Description",
                Questions = new List<CreateQuestionDto>
                {
                    new()
                    {
                        Text = "Question",
                        CorrectChoiceIndex = -1,
                        Choices = new List<CreateChoiceDto>
                        {
                            new() { Text = "A" }, new() { Text = "B" }
                        }
                    }
                }
            };

            var result = await CreateService().CreateQuizAsync(dto);

            Assert.Equal(400, result.StatusCode);
            _unitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            _quizRepository.Verify(r => r.AddAsync(It.IsAny<Quiz>()), Times.Never);
        }

        [Fact]
        public async Task CreateQuiz_WhenDescriptionIsMissing_Returns400WithoutTransaction()
        {
            var dto = new CreateQuizDto
            {
                CourseId = CourseId,
                Name = "Quiz",
                Description = " ",
                Questions = new List<CreateQuestionDto>
                {
                    new()
                    {
                        Text = "Question",
                        CorrectChoiceIndex = 0,
                        Choices = new List<CreateChoiceDto>
                        {
                            new() { Text = "A" }, new() { Text = "B" }
                        }
                    }
                }
            };

            var result = await CreateService().CreateQuizAsync(dto);

            Assert.Equal(400, result.StatusCode);
            _unitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateQuiz_WhenSaveThrows_RollsBackWithoutCommit()
        {
            var dto = new CreateQuizDto
            {
                CourseId = CourseId,
                Name = "Quiz",
                Description = "Description",
                Questions = new List<CreateQuestionDto>
                {
                    new()
                    {
                        Text = "Question",
                        CorrectChoiceIndex = 0,
                        Choices = new List<CreateChoiceDto>
                        {
                            new() { Text = "A" }, new() { Text = "B" }
                        }
                    }
                }
            };
            _unitOfWork.Setup(u => u.Complete()).ThrowsAsync(new InvalidOperationException("save failed"));

            var result = await CreateService().CreateQuizAsync(dto);

            Assert.Equal(500, result.StatusCode);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }
    }
}
