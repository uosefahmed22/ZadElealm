namespace ZadElealm.Apis.Controllers.Deketeed
{
    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CourseLanguage { get; set; }
        public int CourseVideosCount { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<VideoDto> Videos { get; set; }
    }

    public class VideoDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public TimeSpan VideoDuration { get; set; }
    }
}
