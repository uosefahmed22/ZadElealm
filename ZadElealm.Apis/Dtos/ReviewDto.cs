using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos
{
    public class ReviewDto
    {
        [Required]
        [MinLength(10, ErrorMessage = "يجب أن تحتوي الرفيو على ١٠ أحرف على الأقل")]
        [MaxLength(1000, ErrorMessage = "يجب ألا تتجاوز الرفيو ١٬٠٠٠ حرف")]
        public string ReviewText { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
