using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public class UpdateEmailDto
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
