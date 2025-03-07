using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Quaries.Certificate
{
    public class DownloadCertificateQuery : BaseQuery<FileResponseForCertificate>
    {
        public int CertificateId { get; }

        public DownloadCertificateQuery(int certificateId)
        {
            CertificateId = certificateId;
        }
    }
}
