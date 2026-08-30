using AdminDashboard.Handlers.HomeHandler;
using AdminDashboard.Queries.HomeQuery;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using Xunit;

namespace ZadElealm.UnitTests.Dashboard;

public sealed class GetDashboardOverviewQueryHandlerTests : DashboardTestBase
{
    [Fact]
    public async Task Handle_ReturnsRealTotalsAndOrdersRecentItems()
    {
        var user = new AppUser
        {
            Id = "user-1",
            UserName = "learner@test.com",
            Email = "learner@test.com",
            DisplayName = "Learner"
        };
        var category = new Category
        {
            Id = 1,
            Name = "Quran",
            Description = "Quran courses",
            ImageUrl = "image.jpg"
        };
        var olderCourse = CreateCourse(1, "Older course", category.Id, new DateTime(2026, 1, 1));
        var latestCourse = CreateCourse(2, "Latest course", category.Id, new DateTime(2026, 2, 1));

        DbContext.Users.Add(user);
        DbContext.Categories.Add(category);
        DbContext.Courses.AddRange(olderCourse, latestCourse);
        DbContext.Enrollments.Add(new Enrollment
        {
            Id = 1,
            AppUserId = user.Id,
            CourseId = latestCourse.Id
        });
        DbContext.Reports.AddRange(
            CreateReport(1, "Solved issue", user.Id, true, new DateTime(2026, 3, 2)),
            CreateReport(2, "Open issue", user.Id, false, new DateTime(2026, 3, 1)),
            CreateReport(3, "Deleted issue", user.Id, false, new DateTime(2026, 3, 3), isDeleted: true));
        await DbContext.SaveChangesAsync();

        var handler = new GetDashboardOverviewQueryHandler(DbContext);

        var result = await handler.Handle(new GetDashboardOverviewQuery(), CancellationToken.None);

        Assert.Equal(1, result.UsersCount);
        Assert.Equal(2, result.CoursesCount);
        Assert.Equal(1, result.CategoriesCount);
        Assert.Equal(1, result.EnrollmentsCount);
        Assert.Equal(1, result.OpenReportsCount);
        Assert.Equal("Latest course", result.RecentCourses[0].Name);
        Assert.Equal("Open issue", result.RecentReports[0].Title);
        Assert.DoesNotContain(result.RecentReports, report => report.Title == "Deleted issue");
    }

    private static Course CreateCourse(int id, string name, int categoryId, DateTime createdAt)
        => new()
        {
            Id = id,
            Name = name,
            Description = "Description",
            Author = "Author",
            CourseLanguage = "Arabic",
            ImageUrl = "image.jpg",
            CategoryId = categoryId,
            CreatedAt = createdAt
        };

    private static Report CreateReport(
        int id,
        string title,
        string userId,
        bool isSolved,
        DateTime createdAt,
        bool isDeleted = false)
        => new()
        {
            Id = id,
            TitleOfTheIssue = title,
            Description = "Description",
            AppUserId = userId,
            IsSolved = isSolved,
            IsDeleted = isDeleted,
            CreatedAt = createdAt
        };
}
