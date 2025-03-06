using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Models
{
    public class Course : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CourseLanguage { get; set; }
        public int CourseVideosCount { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<Video> Videos { get; set; }
        public ICollection<Quiz> Quizzes { get; set; }
        public ICollection<Enrollment> enrollments { get; set; }
    }
}
