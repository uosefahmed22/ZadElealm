using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Controllers.Deketeed
{
    public class CreatePlaylistRequest
    {
        [Required(ErrorMessage = "رابط القائمة مطلوب")]
        public string PlaylistUrl { get; set; }

        [Required(ErrorMessage = "معرف الفئة مطلوب")]
        public int CategoryId { get; set; }

        public string? CourseLanguage { get; set; }
        public string? Author { get; set; }
    }
}
