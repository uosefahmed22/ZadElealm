using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications
{
    public class CertificatesByUserSpecification : BaseSpecification<Core.Models.Certificate>
    {
        public CertificatesByUserSpecification(string userId) : base(x => x.UserId == userId)
        {
        }
    }
}
