using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public sealed class DeleteAccountDto
    {
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; init; } = string.Empty;
    }
}
