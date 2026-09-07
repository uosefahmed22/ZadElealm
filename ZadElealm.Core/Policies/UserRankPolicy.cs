using ZadElealm.Core.Enums;

namespace ZadElealm.Core.Policies;

public static class UserRankPolicy
{
    public const int PointsPerCompletedCourse = 10;
    public const int PointsPerCertificate = 20;
    public const int QuizAverageContributionPercentage = 50;

    public static IReadOnlyList<UserRankTierDefinition> Tiers { get; } =
    [
        new(UserRankEnum.Bronze, 0, 99),
        new(UserRankEnum.Silver, 100, 299),
        new(UserRankEnum.Gold, 300, 599),
        new(UserRankEnum.Platinum, 600, 999),
        new(UserRankEnum.Diamond, 1000, null)
    ];

    public static int CalculateQuizAverageBonus(double averageQuizScore)
        => (int)(averageQuizScore * QuizAverageContributionPercentage / 100);

    public static UserRankEnum DetermineRank(int points)
        => points <= 0
            ? UserRankEnum.Bronze
            : Tiers.Last(tier => points >= tier.MinimumPoints).Rank;
}

public sealed record UserRankTierDefinition(
    UserRankEnum Rank,
    int MinimumPoints,
    int? MaximumPoints);
