using AdminDashboard.Models;
using MediatR;

namespace AdminDashboard.Quires.CourseQuery;

public sealed class GetAllCoursesQuery : IRequest<CourseIndexViewModel>
{
    public int PageNumber { get; init; } = 1;
    public string? Search { get; init; }
    public string? Language { get; init; }
    public string? Category { get; init; }
    public decimal? MinimumRating { get; init; }
}
