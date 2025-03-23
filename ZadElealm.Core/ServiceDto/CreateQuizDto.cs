using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.ServiceDto
{
    public class CreateQuizDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Quiz must have at least one question")]
        public List<CreateQuestionDto> Questions { get; set; }
    }
}
