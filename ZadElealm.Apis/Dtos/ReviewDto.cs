using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos
{
    public class ReviewDto
    {
        [MinLength(10, ErrorMessage = "يجب أن تحتوي الرفيو على 10 أحرف على الأقل")]
        [MaxLength(1000, ErrorMessage = "يجب ألا تتجاوز الرفيو 1000 حرف")]
        public string ReviewText { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
