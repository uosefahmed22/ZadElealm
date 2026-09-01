using Moq;
using Xunit;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Handlers.Notification;
using ZadElealm.Apis.Quaries.Notification;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.UnitTests.Handlers;

public class GetUserNotificationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_UsesJoinEntityReadStateWithoutMarkingNotificationsAsRead()
    {
        var notification = new Notification
        {
            Id = 7,
            Title = "New lesson",
            Description = "A lesson was added"
        };
        var userNotification = new UserNotification
        {
            Notification = notification,
            NotificationId = notification.Id,
            AppUserId = "user-1",
            IsRead = false
        };
        IReadOnlyList<UserNotification> userNotifications = [userNotification];

        var repository = new Mock<IGenericRepository<UserNotification>>();
        repository
            .Setup(r => r.GetAllWithSpecNoTrackingAsync(It.IsAny<ISpecification<UserNotification>>()))
            .ReturnsAsync(userNotifications);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<UserNotification>()).Returns(repository.Object);
        unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);

        var handler = new GetUserNotificationsQueryHandler(unitOfWork.Object);

        var result = await handler.Handle(
            new GetUserNotificationsQuery("user-1"),
            CancellationToken.None);

        var dataResponse = Assert.IsType<ApiDataResponse>(result);
        var response = Assert.IsType<NotificationsResponse>(dataResponse.Data);
        var dto = Assert.Single(response.Notifications);

        // IsRead comes from UserNotification.IsRead (false at mapping time)
        Assert.False(dto.IsRead);
        Assert.Equal(1, response.UnreadCount);
        Assert.Equal(1, response.TotalCount);

        Assert.False(userNotification.IsRead);
        unitOfWork.Verify(u => u.Complete(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenThereAreNoNotifications_ReturnsAnEmptyStableResponse()
    {
        var repository = new Mock<IGenericRepository<UserNotification>>();
        repository
            .Setup(r => r.GetAllWithSpecNoTrackingAsync(It.IsAny<ISpecification<UserNotification>>()))
            .ReturnsAsync([]);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<UserNotification>()).Returns(repository.Object);

        var result = await new GetUserNotificationsQueryHandler(unitOfWork.Object).Handle(
            new GetUserNotificationsQuery("user-1"),
            CancellationToken.None);

        var dataResponse = Assert.IsType<ApiDataResponse>(result);
        var response = Assert.IsType<NotificationsResponse>(dataResponse.Data);
        Assert.Empty(response.Notifications);
        Assert.Equal(0, response.UnreadCount);
        Assert.Equal(0, response.TotalCount);
    }
}
