using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class Report : BaseEntity
    {
        public string TitleOfTheIssue { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string UserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
