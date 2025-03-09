using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos
{
    public class ReportDto
    {
        [Required(ErrorMessage = "عنوان المشكلة مطلوب")]
        [MaxLength(100, ErrorMessage = "يجب ألا يتجاوز العنوان 100 حرف")]
        public string TitleOfTheIssue { get; set; }

        [Required(ErrorMessage = "وصف المشكلة مطلوب")]
        [MinLength(10, ErrorMessage = "يجب أن يحتوي الوصف على 10 أحرف على الأقل")]
        [MaxLength(1000, ErrorMessage = "يجب ألا يتجاوز الوصف 1000 حرف")]
        public string Description { get; set; }
    }
}
