using System.ComponentModel.DataAnnotations;

namespace ZadElealm.Apis.Dtos
{
    public class RatingDto
    {
        [Required]
        [Range(1, 5)]
        public int Value { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
