using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public class ResetPasswordDTO
    {
        [EmailAddress]
        public string Email { get; set; }
        public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
