using Xunit;
using Moq;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Handlers.QuizHandler;
using ZadElealm.Apis.Quaries.QuizQuery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Quiz;
using ZadElealm.Core.Service;

namespace ZadElealm.UnitTests.Handlers
{
    public class GetQuizQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IVideoProgressService> _videoProgressService = new();
        private readonly Mock<IGenericRepository<Quiz>> _quizRepository = new();

        private GetQuizQueryHandler CreateHandler()
        {
            _unitOfWork.Setup(u => u.Repository<Quiz>()).Returns(_quizRepository.Object);
            return new GetQuizQueryHandler(_unitOfWork.Object, _videoProgressService.Object);
        }

        private static Quiz BuildQuiz(int courseId = 5)
            => new Quiz
            {
                Id = 1,
                Name = "Quiz 1",
                CourseId = courseId,
                Questions = new List<Question>()
            };

        [Fact]
        public async Task Handle_WhenQuizNotFound_Returns404()
        {
            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(
                    It.IsAny<ISpecification<Quiz>>(),
                    CancellationToken.None))
                .ReturnsAsync((Quiz)null!);

            var result = await CreateHandler().Handle(new GetQuizQuery(1, "user-1"), CancellationToken.None);

            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Handle_WhenUserIsNotEligible_Returns403()
        {
            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(
                    It.IsAny<ISpecification<Quiz>>(),
                    CancellationToken.None))
                .ReturnsAsync(BuildQuiz());
            _videoProgressService.Setup(v => v.CheckCourseCompletionEligibilityAsync(
                    "user-1",
                    5,
                    CancellationToken.None))
                .ReturnsAsync(false);

            var result = await CreateHandler().Handle(new GetQuizQuery(1, "user-1"), CancellationToken.None);

            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task Handle_WhenUserIsEligible_Returns200WithQuizDto()
        {
            var quiz = BuildQuiz();

            _quizRepository.Setup(r => r.GetEntityWithSpecNoTrackingAsync(
                    It.IsAny<ISpecification<Quiz>>(),
                    CancellationToken.None))
                .ReturnsAsync(quiz);
            _videoProgressService.Setup(v => v.CheckCourseCompletionEligibilityAsync(
                    "user-1",
                    5,
                    CancellationToken.None))
                .ReturnsAsync(true);

            var result = await CreateHandler().Handle(new GetQuizQuery(1, "user-1"), CancellationToken.None);

            Assert.Equal(200, result.StatusCode);
            var dataResponse = Assert.IsType<ApiDataResponse>(result);
            var dto = Assert.IsType<QuizResponseDto>(dataResponse.Data);
            Assert.Equal(quiz.Id, dto.Id);
            Assert.Equal(quiz.Name, dto.Name);
        }
    }
}
