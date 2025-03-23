using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Models
{
    public class Quiz : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; }
        public ICollection<Progress> Progresses { get; set; }
        public int CourseId { get; set; } 
        public Course Course { get; set; }
        public ICollection<Certificate> Certificates { get; set; } 
    }
}
