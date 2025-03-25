using ZadElealm.Apis.Dtos.DtosCategory;

namespace AdminDashboard.Dto
{
    public class DashboardCourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CourseLanguage { get; set; }
        public int CourseVideosCount { get; set; }
        public decimal rating { get; set; }
        public string? Image { get; set; }
        public IFormFile? formFile { get; set; }
        public CategoryResponseDto Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
