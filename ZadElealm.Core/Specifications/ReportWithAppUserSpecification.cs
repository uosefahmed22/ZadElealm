using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class ReportWithAppUserSpecification : BaseSpecification<Report>
    {
        public ReportWithAppUserSpecification(int reportId) : base(r => r.Id == reportId)
        {
            Includes.Add(r => r.AppUser);
        }
    }
}
