using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class CertificateByIdSpecification : BaseSpecification<Certificate>
    {
        public CertificateByIdSpecification(int certificateId)
            : base(c => c.Id == certificateId)
        {
            Includes.Add(c => c.User);
            Includes.Add(c => c.Quiz);
        }
    }
}
