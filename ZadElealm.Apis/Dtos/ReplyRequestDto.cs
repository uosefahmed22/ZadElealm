using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos
{
    public class ReplyRequestDto
    {
        [Required]
        [StringLength(500, MinimumLength = 2)]
        public string ReplyText { get; set; } = string.Empty;
    }
}
