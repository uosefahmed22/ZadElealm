using AdminDashboard.Dto;
using AdminDashboard.Models;
using AdminDashboard.Quires.CourseQuery;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Repository.Data.Datbases;

namespace AdminDashboard.Handlers.CourseHandler;

public sealed class GetAllCoursesQueryHandler
    : IRequestHandler<GetAllCoursesQuery, CourseIndexViewModel>
{
    private readonly AppDbContext _dbContext;

    public GetAllCoursesQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourseIndexViewModel> Handle(
        GetAllCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var allCourses = _dbContext.Courses.AsNoTracking();
        var filteredCourses = allCourses;

        var search = request.Search?.Trim();
        var language = request.Language?.Trim();
        var category = request.Category?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredCourses = filteredCourses.Where(course =>
                course.Name.Contains(search) ||
                course.Description.Contains(search) ||
                course.Author.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            filteredCourses = filteredCourses.Where(course => course.CourseLanguage == language);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filteredCourses = filteredCourses.Where(course => course.Category.Name == category);
        }

        if (request.MinimumRating.HasValue)
        {
            filteredCourses = filteredCourses.Where(course => course.rating >= request.MinimumRating.Value);
        }

        var totalItems = await filteredCourses.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)CourseIndexViewModel.CoursesPerPage));
        var pageNumber = Math.Clamp(request.PageNumber, 1, totalPages);

        var courses = await filteredCourses
            .OrderByDescending(course => course.CreatedAt)
            .ThenByDescending(course => course.Id)
            .Skip((pageNumber - 1) * CourseIndexViewModel.CoursesPerPage)
            .Take(CourseIndexViewModel.CoursesPerPage)
            .Select(course => new DashboardCourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Author = course.Author,
                CourseLanguage = course.CourseLanguage,
                CourseVideosCount = course.CourseVideosCount,
                rating = course.rating,
                Image = course.ImageUrl,
                CreatedAt = course.CreatedAt,
                Category = new CategoryResponseDto
                {
                    Id = course.Category.Id,
                    Name = course.Category.Name,
                    Description = course.Category.Description,
                    ImageUrl = course.Category.ImageUrl
                }
            })
            .ToListAsync(cancellationToken);

        var languages = await allCourses
            .Where(course => course.CourseLanguage != null && course.CourseLanguage != string.Empty)
            .Select(course => course.CourseLanguage)
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync(cancellationToken);

        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Select(item => item.Name)
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync(cancellationToken);

        var totalCourses = await allCourses.CountAsync(cancellationToken);
        var totalVideos = await allCourses.SumAsync(course => (int?)course.CourseVideosCount, cancellationToken) ?? 0;
        var averageRating = await allCourses.AverageAsync(course => (decimal?)course.rating, cancellationToken) ?? 0;

        return new CourseIndexViewModel
        {
            Courses = courses,
            Languages = languages,
            Categories = categories,
            Search = search,
            Language = language,
            Category = category,
            MinimumRating = request.MinimumRating,
            PageNumber = pageNumber,
            TotalItems = totalItems,
            TotalCourses = totalCourses,
            TotalVideos = totalVideos,
            AverageRating = averageRating
        };
    }
}
