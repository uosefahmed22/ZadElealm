using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class Report : BaseEntity
    {
        public string TitleOfTheIssue { get; set; }
        public string Description { get; set; }
        public ReportType reportTypes { get; set; }
        public string? ImageUrl { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public string? AdminResponse { get; set; }
        public bool IsSolved { get; set; } = false;
    }
}
