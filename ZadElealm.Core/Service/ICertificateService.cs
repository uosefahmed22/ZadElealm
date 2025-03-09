using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Service
{
    public interface ICertificateService
    {
        Task<Certificate> GenerateAndSaveCertificate(string userId, int quizId);
    }
}
