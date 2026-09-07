using Xunit;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Policies;

namespace ZadElealm.UnitTests.Services;

public sealed class UserRankPolicyTests
{
    [Theory]
    [InlineData(0, UserRankEnum.Bronze)]
    [InlineData(99, UserRankEnum.Bronze)]
    [InlineData(100, UserRankEnum.Silver)]
    [InlineData(299, UserRankEnum.Silver)]
    [InlineData(300, UserRankEnum.Gold)]
    [InlineData(599, UserRankEnum.Gold)]
    [InlineData(600, UserRankEnum.Platinum)]
    [InlineData(999, UserRankEnum.Platinum)]
    [InlineData(1000, UserRankEnum.Diamond)]
    public void DetermineRank_UsesThePublishedTierBoundaries(int points, UserRankEnum expected)
    {
        Assert.Equal(expected, UserRankPolicy.DetermineRank(points));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(69, 34)]
    [InlineData(100, 50)]
    public void CalculateQuizAverageBonus_UsesHalfTheAverageAndFloorsFractions(
        double average,
        int expected)
    {
        Assert.Equal(expected, UserRankPolicy.CalculateQuizAverageBonus(average));
    }
}
