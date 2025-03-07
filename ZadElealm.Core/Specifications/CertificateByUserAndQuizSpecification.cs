using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class CertificateByUserAndQuizSpecification : BaseSpecification<Certificate>
    {
        public CertificateByUserAndQuizSpecification(string userId, int quizId)
            : base(c => c.UserId == userId && c.QuizId == quizId)
        {
            Includes.Add(c => c.Quiz);
            Includes.Add(c => c.User);
        }
    }
}
