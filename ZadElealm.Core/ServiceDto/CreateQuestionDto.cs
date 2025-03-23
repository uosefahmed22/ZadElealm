using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.ServiceDto
{
    public class CreateQuestionDto
    {
        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int CorrectChoiceIndex { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Question must have at least 2 choices")]
        public List<CreateChoiceDto> Choices { get; set; }
    }
}
