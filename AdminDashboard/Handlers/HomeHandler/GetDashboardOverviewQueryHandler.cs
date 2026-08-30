using AdminDashboard.Models;
using AdminDashboard.Queries.HomeQuery;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Repository.Data.Datbases;

namespace AdminDashboard.Handlers.HomeHandler;

public sealed class GetDashboardOverviewQueryHandler
    : IRequestHandler<GetDashboardOverviewQuery, DashboardHomeViewModel>
{
    private readonly AppDbContext _dbContext;

    public GetDashboardOverviewQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardHomeViewModel> Handle(
        GetDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var usersCount = await _dbContext.Users.CountAsync(cancellationToken);
        var coursesCount = await _dbContext.Courses.CountAsync(cancellationToken);
        var categoriesCount = await _dbContext.Categories.CountAsync(cancellationToken);
        var enrollmentsCount = await _dbContext.Enrollments.CountAsync(cancellationToken);
        var openReportsCount = await _dbContext.Reports
            .CountAsync(report => !report.IsSolved, cancellationToken);

        var recentCourses = await _dbContext.Courses
            .AsNoTracking()
            .OrderByDescending(course => course.CreatedAt)
            .Take(4)
            .Select(course => new DashboardRecentCourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Author = course.Author,
                Language = course.CourseLanguage,
                CreatedAt = course.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var recentReports = await _dbContext.Reports
            .AsNoTracking()
            .OrderBy(report => report.IsSolved)
            .ThenByDescending(report => report.CreatedAt)
            .Take(4)
            .Select(report => new DashboardRecentReportViewModel
            {
                Id = report.Id,
                Title = report.TitleOfTheIssue,
                IsSolved = report.IsSolved,
                CreatedAt = report.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new DashboardHomeViewModel
        {
            UsersCount = usersCount,
            CoursesCount = coursesCount,
            CategoriesCount = categoriesCount,
            EnrollmentsCount = enrollmentsCount,
            OpenReportsCount = openReportsCount,
            RecentCourses = recentCourses,
            RecentReports = recentReports
        };
    }
}
