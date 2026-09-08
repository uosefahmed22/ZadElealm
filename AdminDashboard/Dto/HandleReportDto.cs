using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Dto
{
    public class HandleReportDto
    {
        [Required(ErrorMessage = "معرف التقرير مطلوب")]
        public int ReportId { get; set; }

        [Required(ErrorMessage = "الرد مطلوب")]
        [MinLength(10, ErrorMessage = "يجب أن يحتوي الرد على ١٠ أحرف على الأقل")]
        [MaxLength(2000, ErrorMessage = "يجب ألا يتجاوز الرد ٢٠٠٠ حرف")]
        public string AdminResponse { get; set; }
    }
}
