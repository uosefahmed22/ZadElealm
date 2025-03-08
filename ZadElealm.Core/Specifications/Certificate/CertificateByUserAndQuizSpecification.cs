using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Certificate
{
    public class CertificateByUserAndQuizSpecification : BaseSpecification<Core.Models.Certificate>
    {
        public CertificateByUserAndQuizSpecification(string userId, int quizId)
            : base(c => c.UserId == userId && c.QuizId == quizId)
        {
            Includes.Add(c => c.Quiz);
            Includes.Add(c => c.User);
        }
    }
}
