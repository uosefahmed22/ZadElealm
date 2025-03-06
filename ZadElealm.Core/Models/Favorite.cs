using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class Favorite : BaseEntity
    {
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string UserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
