using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public class UpdateProfileDto
    {
        [RegularExpression(@"^[\u0600-\u06FF\s]+$",
            ErrorMessage = "يجب إدخال الاسم باللغة العربية فقط")]
        [StringLength(100, MinimumLength = 2)]
        public string? DisplayName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
