using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ZadElealm.IntegrationTests;

public class CourseCatalogContractTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly HttpClient _client;

    public CourseCatalogContractTests(ZadElealmApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetCourses_WithoutCategory_ReturnsAllCoursesInPaginatedContract()
    {
        var response = await _client.GetAsync(
            "/api/Category/get-courses-by-category?pageNumber=1&pageSize=10&sortBy=date&sortDirection=desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.Equal(JsonValueKind.Array, root.GetProperty("data").ValueKind);
        Assert.Equal(2, root.GetProperty("data").GetArrayLength());
        Assert.Equal(2, root.GetProperty("metaData").GetProperty("totalMatchedItems").GetInt32());
        Assert.Equal("مواقف من السيرة", root.GetProperty("data")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetCourses_WithCategoryAndSearch_FiltersTheCatalog()
    {
        var categoriesResponse = await _client.GetAsync("/api/Category");
        Assert.Equal(HttpStatusCode.OK, categoriesResponse.StatusCode);

        using var categoriesDocument = JsonDocument.Parse(
            await categoriesResponse.Content.ReadAsStringAsync());
        var quranCategory = categoriesDocument.RootElement.GetProperty("data")
            .EnumerateArray()
            .Single(category => category.GetProperty("name").GetString() == "القرآن الكريم");
        var categoryId = quranCategory.GetProperty("id").GetInt32();

        var response = await _client.GetAsync(
            $"/api/Category/get-courses-by-category?categoryId={categoryId}&search={Uri.EscapeDataString("تجويد")}&sortBy=name&pageNumber=1&pageSize=9");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var courses = document.RootElement.GetProperty("data");
        Assert.Single(courses.EnumerateArray());
        Assert.Equal("أساسيات التجويد", courses[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetAllCourses_WithMixedOrdering_IsStableAcrossPagesAndVariesCategories()
    {
        var firstPageResponse = await _client.GetAsync(
            "/api/Category/get-courses-by-category?pageNumber=1&pageSize=1&sortBy=mixed");
        var secondPageResponse = await _client.GetAsync(
            "/api/Category/get-courses-by-category?pageNumber=2&pageSize=1&sortBy=mixed");

        Assert.Equal(HttpStatusCode.OK, firstPageResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);

        using var firstDocument = JsonDocument.Parse(
            await firstPageResponse.Content.ReadAsStringAsync());
        using var secondDocument = JsonDocument.Parse(
            await secondPageResponse.Content.ReadAsStringAsync());
        var firstCourse = firstDocument.RootElement.GetProperty("data")[0];
        var secondCourse = secondDocument.RootElement.GetProperty("data")[0];

        Assert.NotEqual(
            firstCourse.GetProperty("id").GetInt32(),
            secondCourse.GetProperty("id").GetInt32());
        Assert.NotEqual(
            firstCourse.GetProperty("category").GetProperty("id").GetInt32(),
            secondCourse.GetProperty("category").GetProperty("id").GetInt32());
    }
}
