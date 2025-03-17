using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Core.ServiceDto
{
    public class QuizDto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        [Required]
        public int CourseId { get; set; }
        public int QuestionCount { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }
}
