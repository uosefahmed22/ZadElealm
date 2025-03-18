using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Service
{
    public interface ICertificateService
    {
        Task<ApiDataResponse> GenerateAndSaveCertificate(string userId, int quizId);
    }
}
