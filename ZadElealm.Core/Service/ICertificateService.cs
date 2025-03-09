using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Service
{
    public interface ICertificateService
    {
        Task<Core.Models.Certificate> GenerateAndSaveCertificate(string userId, int quizId);
    }
}
