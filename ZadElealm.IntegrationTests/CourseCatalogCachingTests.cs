using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZadElealm.Core.Models;
using ZadElealm.Repository.Data.Datbases;
using Xunit;

namespace ZadElealm.IntegrationTests;

public class CourseCatalogCachingTests
{
    [Fact]
    public async Task GetCourses_ReusesCachedPage_ButDifferentPagingKeyReadsFreshData()
    {
        await using var factory = new ZadElealmApiFactory();
        using var client = CreateClient(factory);
        const string cachedUrl =
            "/api/Category/get-courses-by-category?pageNumber=1&pageSize=10&sortBy=name&sortDirection=asc";

        var firstNames = await GetCourseNamesAsync(client, cachedUrl);
        Assert.Contains("أساسيات التجويد", firstNames);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var course = await dbContext.Courses.SingleAsync(item => item.Name == "أساسيات التجويد");
            course.Name = "نسخة محدثة من التجويد";
            await dbContext.SaveChangesAsync();
        }

        var cachedNames = await GetCourseNamesAsync(client, cachedUrl);
        Assert.Contains("أساسيات التجويد", cachedNames);
        Assert.DoesNotContain("نسخة محدثة من التجويد", cachedNames);

        var freshNames = await GetCourseNamesAsync(
            client,
            "/api/Category/get-courses-by-category?pageNumber=1&pageSize=9&sortBy=name&sortDirection=asc");
        Assert.Contains("نسخة محدثة من التجويد", freshNames);
    }

    [Fact]
    public async Task GetCategories_ReusesCachedCategoryList()
    {
        await using var factory = new ZadElealmApiFactory();
        using var client = CreateClient(factory);

        var firstNames = await GetCategoryNamesAsync(client);
        Assert.DoesNotContain("تصنيف أضيف بعد الكاش", firstNames);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Categories.Add(new Category
            {
                Name = "تصنيف أضيف بعد الكاش",
                Description = "يستخدم لإثبات أن القراءة الثانية جاءت من الكاش",
                ImageUrl = "https://example.test/cached-category.jpg",
                Courses = []
            });
            await dbContext.SaveChangesAsync();
        }

        var cachedNames = await GetCategoryNamesAsync(client);
        Assert.DoesNotContain("تصنيف أضيف بعد الكاش", cachedNames);
    }

    private static HttpClient CreateClient(ZadElealmApiFactory factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static async Task<string[]> GetCourseNamesAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(item => item.GetProperty("name").GetString()!)
            .ToArray();
    }

    private static async Task<string[]> GetCategoryNamesAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/Category");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(item => item.GetProperty("name").GetString()!)
            .ToArray();
    }
}
