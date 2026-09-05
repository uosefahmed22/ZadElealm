namespace ZadElealm.Core.ServiceDto;

public sealed class EnrolledCourseProgressReadModel
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public string CourseDescription { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string CourseLanguage { get; init; } = string.Empty;
    public int CourseVideosCount { get; init; }
    public decimal Rating { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryDescription { get; init; } = string.Empty;
    public string CategoryImageUrl { get; init; } = string.Empty;
    public int CompletedVideos { get; init; }
    public int TotalVideos { get; init; }
}
