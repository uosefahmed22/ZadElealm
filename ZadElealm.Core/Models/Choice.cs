using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Models
{
    public class Choice : BaseEntity
    {
        public string Text { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
