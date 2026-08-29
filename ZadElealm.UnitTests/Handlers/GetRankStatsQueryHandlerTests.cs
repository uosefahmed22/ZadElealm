using Moq;
using Xunit;
using ZadElealm.Apis.Handlers.UserRankHandler;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace ZadElealm.UnitTests.Handlers;

public class GetRankStatsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCountsForEveryRankIncludingZeroCounts()
    {
        IReadOnlyList<UserRank> userRanks =
        [
            new() { Rank = UserRankEnum.Bronze },
            new() { Rank = UserRankEnum.Bronze },
            new() { Rank = UserRankEnum.Gold }
        ];

        var repository = new Mock<IGenericRepository<UserRank>>();
        repository.Setup(r => r.GetAllWithNoTrackingAsync()).ReturnsAsync(userRanks);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<UserRank>()).Returns(repository.Object);

        var handler = new GetRankStatsQueryHandler(unitOfWork.Object);

        var result = await handler.Handle(new GetRankStatsQuery(), CancellationToken.None);

        Assert.Equal(Enum.GetValues<UserRankEnum>().Length, result.Count);
        Assert.Equal(2, result[UserRankEnum.Bronze]);
        Assert.Equal(1, result[UserRankEnum.Gold]);
        Assert.Equal(0, result[UserRankEnum.Silver]);
        Assert.Equal(0, result[UserRankEnum.Platinum]);
        Assert.Equal(0, result[UserRankEnum.Diamond]);
    }
}
