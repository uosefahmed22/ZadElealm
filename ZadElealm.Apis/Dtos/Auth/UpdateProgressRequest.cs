using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public class UpdateProgressRequest
    {
        [Required]
        public int VideoId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int WatchedSeconds { get; set; }
    }
}
