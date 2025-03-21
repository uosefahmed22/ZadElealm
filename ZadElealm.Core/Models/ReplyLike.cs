using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class ReplyLike : BaseEntity
    {
        public int ReplyId { get; set; }
        public Reply Reply { get; set; }
        public string AppUserId { get; set; }
        public AppUser User { get; set; }
    }
}
