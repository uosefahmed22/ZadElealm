using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using ZadElealm.Apis.Commands.EnrollmentCommands;
using ZadElealm.Apis.Handlers.EnrollentHandler;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Repository.Data.Datbases;
using ZadElealm.Repository.Repositories;
using ZadElealm.Service.AppServices;

namespace ZadElealm.UnitTests.Handlers
{
    public class EnrollCourseCommandHandlerTests : IDisposable
    {
        private readonly AppDbContext _dbContext;

        public EnrollCourseCommandHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _dbContext = new AppDbContext(options);
        }

        private static Course BuildCourse(int id) => new Course
        {
            Id = id,
            Name = "فقه العبادات",
            Description = "Description",
            Author = "Author",
            CourseLanguage = "Arabic",
            ImageUrl = "https://example.com/image.png"
        };

        private EnrollCourseCommandHandler CreateHandler(INotificationService? notificationService = null)
        {
            var unitOfWork = new UnitOfWork(_dbContext);
            return new EnrollCourseCommandHandler(
                notificationService ?? new NotificationService(unitOfWork),
                unitOfWork,
                new EnrollmentWriteRepository(_dbContext));
        }

        [Fact]
        public async Task Handle_WhenCourseNotFound_Returns404()
        {
            var handler = CreateHandler();

            var result = await handler.Handle(new EnrollCourseCommand(999, "user-1"), CancellationToken.None);

            Assert.Equal(404, result.StatusCode);
            Assert.Empty(_dbContext.Enrollments);
        }

        [Fact]
        public async Task Handle_WhenAlreadyEnrolled_Returns400()
        {
            _dbContext.Courses.Add(BuildCourse(1));
            _dbContext.Enrollments.Add(new Enrollment { CourseId = 1, AppUserId = "user-1" });
            _dbContext.SaveChanges();

            var handler = CreateHandler();

            var result = await handler.Handle(new EnrollCourseCommand(1, "user-1"), CancellationToken.None);

            Assert.Equal(400, result.StatusCode);
            Assert.Single(_dbContext.Enrollments);
        }

        [Fact]
        public async Task Handle_WhenSuccessful_PersistsEnrollmentAndNotificationAtomically()
        {
            _dbContext.Courses.Add(BuildCourse(1));
            _dbContext.SaveChanges();
            var saveChangesCount = 0;
            _dbContext.SavingChanges += (_, _) => saveChangesCount++;

            var handler = CreateHandler();

            var result = await handler.Handle(new EnrollCourseCommand(1, "user-1"), CancellationToken.None);

            Assert.Equal(200, result.StatusCode);
            Assert.Single(_dbContext.Enrollments.Where(e => e.CourseId == 1 && e.AppUserId == "user-1"));
            var notification = _dbContext.Notifications.Include(n => n.UserNotifications).Single();
            Assert.Contains("فقه العبادات", notification.Title);
            Assert.Contains("فقه العبادات", notification.Description);
            Assert.Contains(notification.UserNotifications, un => un.AppUserId == "user-1");
            Assert.Equal(1, saveChangesCount);
        }

        [Fact]
        public async Task Handle_WhenEnrollmentWasCancelled_ReactivatesSameEnrollmentAndKeepsProgress()
        {
            _dbContext.Courses.Add(BuildCourse(1));
            var enrollment = new Enrollment
            {
                CourseId = 1,
                AppUserId = "user-1",
                IsDeleted = true,
                UnenrolledAtUtc = DateTime.UtcNow
            };
            _dbContext.Enrollments.Add(enrollment);
            _dbContext.VideoProgresses.Add(new VideoProgress
            {
                UserId = "user-1",
                VideoId = 10,
                CourseId = 1,
                WatchedDuration = TimeSpan.FromSeconds(50),
                IsCompleted = false
            });
            _dbContext.SaveChanges();
            var originalEnrollmentId = enrollment.Id;

            var handler = CreateHandler();

            var result = await handler.Handle(new EnrollCourseCommand(1, "user-1"), CancellationToken.None);

            Assert.Equal(200, result.StatusCode);
            var storedEnrollment = Assert.Single(_dbContext.Enrollments.IgnoreQueryFilters());
            Assert.Equal(originalEnrollmentId, storedEnrollment.Id);
            Assert.False(storedEnrollment.IsDeleted);
            Assert.Null(storedEnrollment.UnenrolledAtUtc);
            Assert.Equal(TimeSpan.FromSeconds(50), Assert.Single(_dbContext.VideoProgresses).WatchedDuration);
        }

        [Fact]
        public async Task Handle_WhenNotificationFails_PropagatesFailure_AndDoesNotReturnSuccess()
        {
            _dbContext.Courses.Add(BuildCourse(1));
            _dbContext.SaveChanges();

            var failingNotificationService = new Mock<INotificationService>();
            failingNotificationService
                .Setup(n => n.AddNotificationAsync(It.IsAny<NotificationServiceDto>()))
                .ThrowsAsync(new InvalidOperationException("notification failure"));

            var handler = CreateHandler(failingNotificationService.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(new EnrollCourseCommand(1, "user-1"), CancellationToken.None));

            Assert.Empty(_dbContext.Notifications);
        }

        public void Dispose() => _dbContext.Dispose();
    }
}
