using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos
{
    public class ReportDto
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "عنوان المشكلة مطلوب")]
        [MaxLength(100, ErrorMessage = "يجب ألا يتجاوز العنوان ١٠٠ حرف")]
        public string TitleOfTheIssue { get; set; }

        [Required(ErrorMessage = "وصف المشكلة مطلوب")]
        [MinLength(10, ErrorMessage = "يجب أن يحتوي الوصف على ١٠ أحرف على الأقل")]
        [MaxLength(1000, ErrorMessage = "يجب ألا يتجاوز الوصف ١٬٠٠٠ حرف")]
        public string Description { get; set; }
        [Required(ErrorMessage = "نوع التقرير مطلوب")]
        public string reportTypes { get; set; }
        public string? AdminResponse { get; set; }
        public bool? IsSolved { get; set; }
    }
}
