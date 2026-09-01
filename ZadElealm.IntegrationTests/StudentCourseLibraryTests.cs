using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
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
    }
}
