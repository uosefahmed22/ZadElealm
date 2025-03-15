using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Dtos
{
    public class QuizResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public CourseDto Course { get; set; }
        public ICollection<QuestionDto> Questions { get; set; }
    }
}
