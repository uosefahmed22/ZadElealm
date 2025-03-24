using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.UserRankquery
{
    public class GetUserRankQuery : IRequest<ApiDataResponse>
    {
        public string UserId { get; set; }
    }

}
