using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.ServiceDto
{
    public class CreateChoiceDto
    {
        [Required]
        [StringLength(200)]
        public string Text { get; set; }
    }
}
