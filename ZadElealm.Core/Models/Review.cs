using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class Review : BaseEntity
    {
        public string Text { get; set; }
        public int CourseId { get; set; } 
        public Course Course { get; set; }
        public string AppUserId { get; set; }
        public AppUser User { get; set; }
        public ICollection<Reply> Replies { get; set; } = new List<Reply>();
        public ICollection<ReviewLike> Likes { get; set; } = new List<ReviewLike>();
        public int LikesCount => Likes.Count;
    }
}
