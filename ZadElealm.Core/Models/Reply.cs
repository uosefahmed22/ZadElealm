using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class Reply : BaseEntity
    {
        public string Text { get; set; }
        public int ReviewId { get; set; }
        public Review Review { get; set; }
        public string AppUserId { get; set; }
        public AppUser User { get; set; }
        public ICollection<ReplyLike> ReplyLikes { get; set; } = new List<ReplyLike>();
        public int ReplyLikesCount => ReplyLikes.Count;
    }
}
