using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public class SendChangeEmailOtpDto
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
