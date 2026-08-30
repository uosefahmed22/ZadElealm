using AdminDashboard.Dto;

namespace AdminDashboard.Models;

public sealed class CourseIndexViewModel
{
    public const int CoursesPerPage = 10;

    public IReadOnlyList<DashboardCourseDto> Courses { get; init; } = [];
    public IReadOnlyList<string> Languages { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public string? Search { get; init; }
    public string? Language { get; init; }
    public string? Category { get; init; }
    public decimal? MinimumRating { get; init; }
    public int PageNumber { get; init; } = 1;
    public int TotalItems { get; init; }
    public int TotalCourses { get; init; }
    public int TotalVideos { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)CoursesPerPage));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
