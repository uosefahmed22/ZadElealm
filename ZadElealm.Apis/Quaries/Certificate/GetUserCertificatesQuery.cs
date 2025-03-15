using AutoMapper;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Quaries.Certificate
{
    public class GetUserCertificatesQuery : BaseQuery<ApiResponse>
    {
        public string UserId { get; }

        public GetUserCertificatesQuery(string userId)
        {
            UserId = userId;
        }
    }

    
}
