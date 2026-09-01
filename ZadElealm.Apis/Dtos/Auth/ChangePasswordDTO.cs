using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public class ChangePasswordDTO
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
