namespace AdminDashboard.Models;

public sealed class DashboardHomeViewModel
{
    public int UsersCount { get; init; }
    public int CoursesCount { get; init; }
    public int CategoriesCount { get; init; }
    public int EnrollmentsCount { get; init; }
    public int OpenReportsCount { get; init; }
    public IReadOnlyList<DashboardRecentCourseViewModel> RecentCourses { get; init; } = [];
    public IReadOnlyList<DashboardRecentReportViewModel> RecentReports { get; init; } = [];
}

public sealed class DashboardRecentCourseViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class DashboardRecentReportViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsSolved { get; init; }
    public DateTime CreatedAt { get; init; }
}
