using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Models;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests;

public class StudentCourseLibraryTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly ZadElealmApiFactory _factory;

    public StudentCourseLibraryTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Student_CanManageEnrollmentAndFavorites_WithStableListContracts()
    {
        var courseId = await GetSeededCourseIdAsync();
        using var client = CreateAuthenticatedClient();

        await AssertEmptyCourseListAsync(client, "/api/Enrollment", "allEnrolledCourses");
        await AssertEmptyCourseListAsync(client, "/api/Favorite", "allFavoriteCourses");

        var addFavorite = await client.PostAsync($"/api/Favorite/{courseId}", null);
        Assert.Equal(HttpStatusCode.OK, addFavorite.StatusCode);

        var duplicateFavorite = await client.PostAsync($"/api/Favorite/{courseId}", null);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateFavorite.StatusCode);

        await AssertCourseListAsync(client, "/api/Favorite", "allFavoriteCourses", courseId);

        var enroll = await client.PostAsync($"/api/Enrollment/{courseId}", null);
        Assert.Equal(HttpStatusCode.OK, enroll.StatusCode);
        await AssertCourseListAsync(client, "/api/Enrollment", "allEnrolledCourses", courseId);

        var unenroll = await client.DeleteAsync($"/api/Enrollment/{courseId}");
        Assert.Equal(HttpStatusCode.OK, unenroll.StatusCode);
        await AssertEmptyCourseListAsync(client, "/api/Enrollment", "allEnrolledCourses");

        var duplicateUnenroll = await client.DeleteAsync($"/api/Enrollment/{courseId}");
        Assert.Equal(HttpStatusCode.NotFound, duplicateUnenroll.StatusCode);

        var removeFavorite = await client.DeleteAsync($"/api/Favorite/{courseId}");
        Assert.Equal(HttpStatusCode.OK, removeFavorite.StatusCode);
        await AssertEmptyCourseListAsync(client, "/api/Favorite", "allFavoriteCourses");

        var duplicateRemove = await client.DeleteAsync($"/api/Favorite/{courseId}");
        Assert.Equal(HttpStatusCode.NotFound, duplicateRemove.StatusCode);
    }

    [Theory]
    [InlineData("/api/Enrollment")]
    [InlineData("/api/Favorite")]
    public async Task StudentCourseLists_RequireAuthentication(string endpoint)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EnrollmentList_IncludesCategoryAndBatchedProgress()
    {
        var seeded = await SeedCompletedVideoAsync();
        using var client = CreateAuthenticatedClient();

        var enroll = await client.PostAsync($"/api/Enrollment/{seeded.CourseId}", null);
        Assert.Equal(HttpStatusCode.OK, enroll.StatusCode);

        var response = await client.GetAsync("/api/Enrollment");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var course = Assert.Single(data.GetProperty("courses").EnumerateArray());
        Assert.Equal("القرآن الكريم", course.GetProperty("category").GetProperty("name").GetString());

        var progress = Assert.Single(data.GetProperty("progress").EnumerateArray());
        Assert.Equal(seeded.CourseId, progress.GetProperty("courseId").GetInt32());
        Assert.Equal(1, progress.GetProperty("completedVideos").GetInt32());
        Assert.Equal(1, progress.GetProperty("totalVideos").GetInt32());
        Assert.Equal(100, progress.GetProperty("overallProgress").GetSingle());

        var unenroll = await client.DeleteAsync($"/api/Enrollment/{seeded.CourseId}");
        Assert.Equal(HttpStatusCode.OK, unenroll.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateToken("User"));
        return client;
    }

    private async Task<int> GetSeededCourseIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Courses
            .OrderBy(course => course.Id)
            .Select(course => course.Id)
            .FirstAsync();
    }

    private async Task<(int CourseId, int VideoId)> SeedCompletedVideoAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var courseId = await dbContext.Courses
            .OrderBy(course => course.Id)
            .Select(course => course.Id)
            .FirstAsync();
        var userId = await dbContext.Users
            .Where(user => user.Email == "user@test.com")
            .Select(user => user.Id)
            .SingleAsync();

        var video = new Video
        {
            Title = "اختبار تقدم الدورة",
            Description = "فيديو مخصص لاختبار عقد التقدم المجمع",
            VideoUrl = "https://example.test/video",
            ThumbnailUrl = "https://example.test/video.jpg",
            VideoDuration = TimeSpan.FromMinutes(5),
            OrderInCourse = 1,
            CourseId = courseId
        };
        dbContext.Videos.Add(video);
        await dbContext.SaveChangesAsync();

        dbContext.VideoProgresses.Add(new VideoProgress
        {
            UserId = userId,
            VideoId = video.Id,
            CourseId = courseId,
            WatchedDuration = video.VideoDuration,
            IsCompleted = true
        });
        await dbContext.SaveChangesAsync();

        return (courseId, video.Id);
    }

    private static async Task AssertEmptyCourseListAsync(
        HttpClient client,
        string endpoint,
        string countProperty)
    {
        var response = await client.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.Empty(data.GetProperty("courses").EnumerateArray());
        Assert.Equal(0, data.GetProperty(countProperty).GetInt32());
    }

    private static async Task AssertCourseListAsync(
        HttpClient client,
        string endpoint,
        string countProperty,
        int expectedCourseId)
    {
        var response = await client.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty(countProperty).GetInt32());
        var course = Assert.Single(data.GetProperty("courses").EnumerateArray());
        Assert.Equal(expectedCourseId, course.GetProperty("id").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(course.GetProperty("imageUrl").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(
            course.GetProperty("category").GetProperty("name").GetString()));
    }
}
