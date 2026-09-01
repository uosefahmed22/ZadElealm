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
