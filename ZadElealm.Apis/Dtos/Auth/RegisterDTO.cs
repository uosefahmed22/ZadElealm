﻿using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos.Auth
{
    public class RegisterDTO
    {
        [Required]
        [RegularExpression(@"^[\u0600-\u06FF\s]+$",
            ErrorMessage = "يجب إدخال الاسم باللغة العربية فقط")]
        public string DisplayName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
