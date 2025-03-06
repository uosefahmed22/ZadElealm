using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Service
{
    public interface ISendEmailService
    {
        Task SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
    }
}
