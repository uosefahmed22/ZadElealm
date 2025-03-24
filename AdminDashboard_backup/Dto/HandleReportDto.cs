using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Dto
{
    public class HandleReportDto
    {
        [Required(ErrorMessage = "معرف التقرير مطلوب")]
        public int ReportId { get; set; }

        [Required(ErrorMessage = "الرد مطلوب")]
        [MinLength(10, ErrorMessage = "يجب أن يحتوي الرد على 10 أحرف على الأقل")]
        public string AdminResponse { get; set; }
    }
}
