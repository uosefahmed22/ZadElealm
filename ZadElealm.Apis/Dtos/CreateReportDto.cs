using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos
{
    public sealed class CreateReportDto
    {
        [Required(ErrorMessage = "عنوان المشكلة مطلوب")]
        [MaxLength(100, ErrorMessage = "يجب ألا يتجاوز العنوان ١٠٠ حرف")]
        public string TitleOfTheIssue { get; set; } = string.Empty;

        [Required(ErrorMessage = "وصف المشكلة مطلوب")]
        [MinLength(10, ErrorMessage = "يجب أن يحتوي الوصف على ١٠ أحرف على الأقل")]
        [MaxLength(1000, ErrorMessage = "يجب ألا يتجاوز الوصف ١٬٠٠٠ حرف")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع البلاغ مطلوب")]
        [RegularExpression(
            "^(Technical|CustomerService|ProductIssue|Other)$",
            ErrorMessage = "نوع البلاغ غير صالح")]
        public string ReportType { get; set; } = string.Empty;
    }
}
