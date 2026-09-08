using Moq;
using Xunit;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Service.AppServices;

namespace ZadElealm.UnitTests.Services;

public class VideoProgressServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IGenericRepository<Video>> _videoRepository = new();
    private readonly Mock<IGenericRepository<VideoProgress>> _progressRepository = new();
    private readonly Mock<IGenericRepository<Course>> _courseRepository = new();
    private readonly Mock<IGenericRepository<Enrollment>> _enrollmentRepository = new();
    private readonly Mock<IVideoProgressReadRepository> _videoProgressReadRepository = new();

    public VideoProgressServiceTests()
    {
        _unitOfWork.Setup(u => u.Repository<Video>()).Returns(_videoRepository.Object);
        _unitOfWork.Setup(u => u.Repository<VideoProgress>()).Returns(_progressRepository.Object);
        _unitOfWork.Setup(u => u.Repository<Course>()).Returns(_courseRepository.Object);
        _unitOfWork.Setup(u => u.Repository<Enrollment>()).Returns(_enrollmentRepository.Object);
    }

    [Fact]
    public async Task UpdateProgress_WhenVideoDurationIsZero_Returns400WithoutSaving()
    {
        _videoRepository.Setup(r => r.GetEntityWithNoTrackingAsync(1, CancellationToken.None))
            .ReturnsAsync(new Video { Id = 1, CourseId = 2, VideoDuration = TimeSpan.Zero });

        var result = await CreateService()
            .UpdateProgressAsync("user-1", 1, TimeSpan.Zero);

        Assert.Equal(400, result.StatusCode);
        _unitOfWork.Verify(u => u.Complete(), Times.Never);
    }

    [Fact]
    public async Task GetVideoProgress_ReturnsTheStoredEntityInsteadOfTheSpecification()
    {
        var progress = new VideoProgress { Id = 4, UserId = "user-1", VideoId = 7, CourseId = 2 };
        _progressRepository
            .Setup(r => r.GetEntityWithSpecNoTrackingAsync(
                It.IsAny<ISpecification<VideoProgress>>(),
                CancellationToken.None))
            .ReturnsAsync(progress);

        var result = await CreateService()
            .GetVideoProgressAsync("user-1", 7);

        Assert.Equal(200, result.StatusCode);
        Assert.Same(progress, result.Data);
    }

    [Fact]
    public async Task GetCourseProgress_UsesCountQueriesAndClampsDuplicateCompletedRows()
    {
        _videoProgressReadRepository
            .Setup(r => r.GetCourseSummaryAsync("user-1", 2, CancellationToken.None))
            .ReturnsAsync(new CourseProgressSummaryReadModel(true, 3, 5));

        var result = await CreateService()
            .GetCourseProgressAsync("user-1", 2);

        var progress = Assert.IsType<CourseProgress>(result.Data);
        Assert.Equal(3, progress.CompletedVideos);
        Assert.Equal(100, progress.VideoProgress);
        _progressRepository.Verify(r => r.GetAllWithSpecNoTrackingAsync(It.IsAny<ISpecification<VideoProgress>>()), Times.Never);
    }

    private VideoProgressService CreateService()
        => new(_unitOfWork.Object, _videoProgressReadRepository.Object);
}
