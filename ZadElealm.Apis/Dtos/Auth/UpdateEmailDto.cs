using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public class UpdateEmailDto
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = string.Empty;
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
