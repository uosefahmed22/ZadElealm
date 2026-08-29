using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Quaries.UserRankquery
{
    public class GetUserRankQuery : IRequest<ApiDataResponse>
    {
        public string UserId { get; set; }
    }

}
