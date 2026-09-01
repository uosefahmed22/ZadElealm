using Moq;
using Xunit;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Service.AppServices;

namespace ZadElealm.UnitTests.Services;

public class UserRankCalculatorTests
{
    [Fact]
    public async Task CalculatePoints_UsesScalarCertificateCountAndUpdatesAllRankFields()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var progressRepository = new Mock<IGenericRepository<Progress>>();
        var certificateRepository = new Mock<IGenericRepository<Certificate>>();
        var rankRepository = new Mock<IGenericRepository<UserRank>>();
        var rank = new UserRank { Id = 1, UserId = "user-1" };
        var course = new Course { Id = 8 };
        var progresses = new List<Progress>
        {
            new() { Score = 70, IsCompleted = true, Quiz = new Quiz { CourseId = course.Id } },
            new() { Score = 90, IsCompleted = true, Quiz = new Quiz { CourseId = course.Id } }
        };

        unitOfWork.Setup(u => u.Repository<Progress>()).Returns(progressRepository.Object);
        unitOfWork.Setup(u => u.Repository<Certificate>()).Returns(certificateRepository.Object);
        unitOfWork.Setup(u => u.Repository<UserRank>()).Returns(rankRepository.Object);
        progressRepository
            .Setup(r => r.GetAllWithSpecNoTrackingAsync(It.IsAny<ISpecification<Progress>>()))
            .ReturnsAsync(progresses);
        certificateRepository.Setup(r => r.CountAsync(It.IsAny<ISpecification<Certificate>>()))
            .ReturnsAsync(2);
        rankRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<UserRank>>()))
            .ReturnsAsync(rank);

        var points = await new UserRankCalculator(unitOfWork.Object).CalculatePoints("user-1");

        Assert.Equal(90, points);
        Assert.Equal(90, rank.TotalPoints);
        Assert.Equal(UserRankEnum.Bronze, rank.Rank);
        Assert.Equal(1, rank.CompletedCoursesCount);
        Assert.Equal(2, rank.CertificatesCount);
        Assert.Equal(80, rank.AverageQuizScore);
        Assert.NotEqual(default, rank.LastUpdated);
        certificateRepository.Verify(
            r => r.GetAllWithSpecNoTrackingAsync(It.IsAny<ISpecification<Certificate>>()), Times.Never);
        unitOfWork.Verify(u => u.Complete(), Times.Once);
    }

    [Fact]
    public async Task CalculatePoints_CreatesACompleteRankWhenTheUserHasNoPreviousRow()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var progressRepository = new Mock<IGenericRepository<Progress>>();
        var certificateRepository = new Mock<IGenericRepository<Certificate>>();
        var rankRepository = new Mock<IGenericRepository<UserRank>>();
        UserRank? createdRank = null;

        unitOfWork.Setup(u => u.Repository<Progress>()).Returns(progressRepository.Object);
        unitOfWork.Setup(u => u.Repository<Certificate>()).Returns(certificateRepository.Object);
        unitOfWork.Setup(u => u.Repository<UserRank>()).Returns(rankRepository.Object);
        progressRepository
            .Setup(r => r.GetAllWithSpecNoTrackingAsync(It.IsAny<ISpecification<Progress>>()))
            .ReturnsAsync([]);
        certificateRepository.Setup(r => r.CountAsync(It.IsAny<ISpecification<Certificate>>()))
            .ReturnsAsync(0);
        rankRepository.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<UserRank>>()))
            .ReturnsAsync((UserRank?)null);
        rankRepository.Setup(r => r.AddAsync(It.IsAny<UserRank>()))
            .Callback<UserRank>(rank => createdRank = rank)
            .Returns(Task.CompletedTask);

        var points = await new UserRankCalculator(unitOfWork.Object).CalculatePoints("new-user");

        Assert.Equal(0, points);
        Assert.NotNull(createdRank);
        Assert.Equal("new-user", createdRank.UserId);
        Assert.Equal(UserRankEnum.Bronze, createdRank.Rank);
        Assert.Equal(0, createdRank.CompletedCoursesCount);
        Assert.Equal(0, createdRank.CertificatesCount);
        Assert.Equal(0, createdRank.AverageQuizScore);
        Assert.NotEqual(default, createdRank.LastUpdated);
        rankRepository.Verify(r => r.AddAsync(It.IsAny<UserRank>()), Times.Once);
        rankRepository.Verify(r => r.Update(It.IsAny<UserRank>()), Times.Never);
        unitOfWork.Verify(u => u.Complete(), Times.Once);
    }
}
