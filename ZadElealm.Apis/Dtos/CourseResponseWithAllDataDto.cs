using ZadElealm.Core.Models;

namespace ZadElealm.Apis.Dtos
{
    public class CourseResponseWithAllDataDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public decimal rating { get; set; }
        public string CourseLanguage { get; set; }
        public int CourseVideosCount { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int TotalEnrolledStudents { get; set; }
        public CategoryResponseDto Category { get; set; }
        public IReadOnlyCollection<VideoDto> Videos { get; set; }
        public ICollection<ReviewWithReviwerDataDto> Review { get; set; }
        public IReadOnlyCollection<QuizResponseForCourseDto> Quizzes { get; set; }
    }
}
