using AdminDashboard.Handlers.CourseHandler;
using AdminDashboard.Quires.CourseQuery;
using ZadElealm.Core.Models;
using Xunit;

namespace ZadElealm.UnitTests.Dashboard;

public sealed class GetAllCoursesQueryHandlerTests : DashboardTestBase
{
    [Fact]
    public async Task Handle_ReturnsTenCoursesPerPageWithCorrectMetadata()
    {
        await SeedCoursesAsync(23);
        var handler = new GetAllCoursesQueryHandler(DbContext);

        var result = await handler.Handle(
            new GetAllCoursesQuery { PageNumber = 2 },
            CancellationToken.None);

        Assert.Equal(10, result.Courses.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(23, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(23, result.TotalCourses);
        Assert.Equal(13, result.Courses[0].Id);
        Assert.Equal(4, result.Courses[^1].Id);
    }

    [Fact]
    public async Task Handle_ClampsPageNumberToLastAvailablePage()
    {
        await SeedCoursesAsync(23);
        var handler = new GetAllCoursesQueryHandler(DbContext);

        var result = await handler.Handle(
            new GetAllCoursesQuery { PageNumber = 99 },
            CancellationToken.None);

        Assert.Equal(3, result.PageNumber);
        Assert.Equal(3, result.Courses.Count);
        Assert.Equal([3, 2, 1], result.Courses.Select(course => course.Id));
    }

    private async Task SeedCoursesAsync(int count)
    {
        var category = new Category
        {
            Id = 1,
            Name = "القرآن الكريم",
            Description = "وصف التصنيف",
            ImageUrl = "category.jpg"
        };
        DbContext.Categories.Add(category);

        for (var id = 1; id <= count; id++)
        {
            DbContext.Courses.Add(new Course
            {
                Id = id,
                Name = $"كورس {id}",
                Description = "وصف الكورس",
                Author = "المحاضر",
                CourseLanguage = "العربية",
                CourseVideosCount = id,
                ImageUrl = "course.jpg",
                CategoryId = category.Id,
                CreatedAt = new DateTime(2026, 1, 1).AddDays(id),
                rating = id % 5
            });
        }

        await DbContext.SaveChangesAsync();
    }
}
