namespace ZadElealm.Apis.Dtos
{
    public sealed class RankDashboardDto
    {
        public StudentRankSummaryDto CurrentUser { get; init; } = new();
        public IReadOnlyList<LeaderboardEntryDto> Leaders { get; init; } = [];
    }

    public sealed class StudentRankSummaryDto
    {
        public int TotalPoints { get; init; }
        public string Rank { get; init; } = string.Empty;
        public int CompletedCoursesCount { get; init; }
        public int CertificatesCount { get; init; }
        public double AverageQuizScore { get; init; }
        public DateTime LastUpdated { get; init; }
    }

    public sealed class LeaderboardEntryDto
    {
        public int Position { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public int TotalPoints { get; init; }
        public string Rank { get; init; } = string.Empty;
        public int CompletedCoursesCount { get; init; }
        public bool IsCurrentUser { get; init; }
    }
}
