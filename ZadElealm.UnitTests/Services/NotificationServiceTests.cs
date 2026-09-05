using Moq;
using Xunit;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Service.AppServices;

namespace ZadElealm.UnitTests.Services;

public class NotificationServiceTests
{
    [Fact]
    public async Task AddNotification_QueuesEntityWithoutSaving()
    {
        var repository = new Mock<IGenericRepository<Notification>>();
        repository.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<Notification>()).Returns(repository.Object);

        var result = await new NotificationService(unitOfWork.Object)
            .AddNotificationAsync(new NotificationServiceDto
            {
                UserId = "user-1",
                Title = "Notification title",
                Description = "Notification description"
            });

        Assert.Equal(200, result.StatusCode);
        repository.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        unitOfWork.Verify(u => u.Complete(), Times.Never);
    }

    [Fact]
    public async Task SendNotification_WhenRequiredDataIsMissing_Returns400WithoutSaving()
    {
        var unitOfWork = new Mock<IUnitOfWork>();

        var result = await new NotificationService(unitOfWork.Object)
            .SendNotificationAsync(new NotificationServiceDto());

        Assert.Equal(400, result.StatusCode);
        unitOfWork.Verify(u => u.Repository<Notification>(), Times.Never);
        unitOfWork.Verify(u => u.Complete(), Times.Never);
    }
}
