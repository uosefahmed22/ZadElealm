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
        IReadOnlyDictionary<UserRankEnum, int> counts =
            new Dictionary<UserRankEnum, int>
            {
                [UserRankEnum.Bronze] = 2,
                [UserRankEnum.Gold] = 1
            };
        var repository = new Mock<IUserRankReadRepository>();
        repository.Setup(r => r.GetCountsByRankAsync(CancellationToken.None))
            .ReturnsAsync(counts);

        var handler = new GetRankStatsQueryHandler(repository.Object);

        var result = await handler.Handle(new GetRankStatsQuery(), CancellationToken.None);

        Assert.Equal(Enum.GetValues<UserRankEnum>().Length, result.Count);
        Assert.Equal(2, result[UserRankEnum.Bronze]);
        Assert.Equal(1, result[UserRankEnum.Gold]);
        Assert.Equal(0, result[UserRankEnum.Silver]);
        Assert.Equal(0, result[UserRankEnum.Platinum]);
        Assert.Equal(0, result[UserRankEnum.Diamond]);
    }
}
