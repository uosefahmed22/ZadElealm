namespace ZadElealm.Core.ServiceDto;

public sealed class AchievementDashboardDto
{
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
    public int UnlockedCount { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyList<string> NewlyUnlocked { get; init; } = [];
    public IReadOnlyList<AchievementItemDto> Achievements { get; init; } = [];
}

public sealed class AchievementItemDto
{
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string IconKey { get; init; } = string.Empty;
    public bool IsUnlocked { get; init; }
    public DateTime? UnlockedAtUtc { get; init; }
    public int CurrentValue { get; init; }
    public int Target { get; init; }
}
