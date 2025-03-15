using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.ServiceDto
{
    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public bool IsCorrect { get; set; }
        public int SelectedChoice { get; set; }
        public int CorrectChoice { get; set; }
    }
}
