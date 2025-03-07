using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Models
{
    public class Question : BaseEntity
    {
        public string Text { get; set; }
        public string? ImageUrl { get; set; }
        public string CorrectChoice { get; set; }
        public int QuizId { get; set; }
        public ICollection<Choice> Choices { get; set; }
        public Quiz Quiz { get; set; }
    }
}
