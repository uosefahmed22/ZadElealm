using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class CertificatesByUserSpecification : BaseSpecification<Certificate>
    {
        public CertificatesByUserSpecification(string userId)
            : base(c => c.UserId == userId)
        {
            Includes.Add(c => c.User);
            Includes.Add(c => c.Quiz);
            AddOrderByDescending(c => c.Compliateddate);
        }
    }
}
