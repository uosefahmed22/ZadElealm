using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class Rating : BaseEntity
    {
        public decimal Value { get; set; }
        public int courseId { get; set; }
        public Course course { get; set; }
        public string AppUserId { get; set; }
        public AppUser User { get; set; }
    }
}
