using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Hybrid;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Core.Specifications.Course;

namespace ZadElealm.Apis.Caching;

public static class CatalogCache
{
    public const string CategoriesKey = "catalog:categories:v1";
    public const string CatalogTag = "catalog";
    public const string CategoriesTag = "catalog:categories";
    public const string CoursesTag = "catalog:courses";

    public static readonly HybridCacheEntryOptions CategoriesOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(15),
        LocalCacheExpiration = TimeSpan.FromMinutes(15)
    };

    public static readonly HybridCacheEntryOptions CoursesOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(3),
        LocalCacheExpiration = TimeSpan.FromMinutes(3)
    };

    public static string GetCoursesKey(CourseSpecParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var normalizedParameters = string.Join('|',
            parameters.CategoryId.ToString(CultureInfo.InvariantCulture),
            Normalize(parameters.Search),
            Normalize(parameters.Author),
            Normalize(parameters.Language),
            FormatDate(parameters.FromDate),
            FormatDate(parameters.ToDate),
            parameters.MinRating?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            parameters.MaxRating?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            Normalize(parameters.SortBy),
            Normalize(parameters.SortDirection),
            parameters.PageNumber.ToString(CultureInfo.InvariantCulture),
            parameters.PageSize.ToString(CultureInfo.InvariantCulture));

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedParameters)));
        return $"catalog:courses:v1:{hash}";
    }

    private static string Normalize(string? value) => value?.Trim().ToLowerInvariant() ?? string.Empty;

    private static string FormatDate(DateTime? value) =>
        value?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
}

public sealed record CourseCatalogCacheEntry(
    IReadOnlyList<CourseDto> Courses,
    int TotalItems);
