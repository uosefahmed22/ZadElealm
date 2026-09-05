using Xunit;
using ZadElealm.Apis.Caching;
using ZadElealm.Core.Specifications.Course;

namespace ZadElealm.UnitTests.Caching;

public class CatalogCacheTests
{
    [Fact]
    public void GetCoursesKey_IncludesEveryCatalogFilterAndPagingValue()
    {
        var baseline = CreateParameters();
        var keys = new[]
        {
            CatalogCache.GetCoursesKey(baseline),
            CatalogCache.GetCoursesKey(CreateParameters(categoryId: 2)),
            CatalogCache.GetCoursesKey(CreateParameters(search: "حديث")),
            CatalogCache.GetCoursesKey(CreateParameters(author: "كاتب آخر")),
            CatalogCache.GetCoursesKey(CreateParameters(language: "english")),
            CatalogCache.GetCoursesKey(CreateParameters(fromDate: new DateTime(2026, 1, 2))),
            CatalogCache.GetCoursesKey(CreateParameters(toDate: new DateTime(2026, 12, 30))),
            CatalogCache.GetCoursesKey(CreateParameters(minRating: 2)),
            CatalogCache.GetCoursesKey(CreateParameters(maxRating: 4)),
            CatalogCache.GetCoursesKey(CreateParameters(sortBy: "date")),
            CatalogCache.GetCoursesKey(CreateParameters(sortDirection: "desc")),
            CatalogCache.GetCoursesKey(CreateParameters(pageNumber: 2)),
            CatalogCache.GetCoursesKey(CreateParameters(pageSize: 20))
        };

        Assert.Equal(keys.Length, keys.Distinct().Count());
    }

    [Fact]
    public void GetCoursesKey_NormalizesEquivalentTextValues()
    {
        var first = CreateParameters(search: "  تجويد  ", sortBy: " NAME ");
        var second = CreateParameters(search: "تجويد", sortBy: "name");

        Assert.Equal(
            CatalogCache.GetCoursesKey(first),
            CatalogCache.GetCoursesKey(second));
    }

    private static CourseSpecParams CreateParameters(
        int categoryId = 1,
        string search = "تجويد",
        string author = "أحمد",
        string language = "العربية",
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? minRating = 3,
        int? maxRating = 5,
        string sortBy = "name",
        string sortDirection = "asc",
        int pageNumber = 1,
        int pageSize = 10)
    {
        return new CourseSpecParams
        {
            CategoryId = categoryId,
            Search = search,
            Author = author,
            Language = language,
            FromDate = fromDate ?? new DateTime(2026, 1, 1),
            ToDate = toDate ?? new DateTime(2026, 12, 31),
            MinRating = minRating,
            MaxRating = maxRating,
            SortBy = sortBy,
            SortDirection = sortDirection,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
