using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class PlaylistViewModel
    {
        [Required]
        public string PlaylistUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? CourseLanguage { get; set; }
        public string? Author { get; set; }
    }
}
