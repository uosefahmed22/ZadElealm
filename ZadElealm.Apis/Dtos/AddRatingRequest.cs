using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos
{
    public class AddRatingRequest
    {
        [Required]
        [Range(1, 5)]
        public int Value { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
