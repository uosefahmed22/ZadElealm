using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models
{
    public class Certificate : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string PdfUrl { get; set; }
        public string UserId { get; set; }
        // Legacy course quizzes still use QuizId. Category assessments use AssessmentId.
        public int? QuizId { get; set; }
        public int? AssessmentId { get; set; }
        public AppUser User { get; set; }
        public Quiz? Quiz { get; set; }
        public Assessment? Assessment { get; set; }
    }
}
