using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Quaries.Certificate;

public sealed class GetCertificateFileQuery : BaseQuery<ApiResponse>
{
    public GetCertificateFileQuery(int certificateId, string userId)
    {
        CertificateId = certificateId;
        UserId = userId;
    }

    public int CertificateId { get; }
    public string UserId { get; }
}
