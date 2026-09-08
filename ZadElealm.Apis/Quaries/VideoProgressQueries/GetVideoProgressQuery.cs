using ZadElealm.Apis.Dtos;

namespace ZadElealm.Apis.Quaries.VideoProgressQueries
{
    public class GetVideoProgressQuery : BaseQuery<ZadElealm.Core.Errors.ApiDataResponse>
    {
        public string UserId { get; set; }
        public int VideoId { get; set; }
    }
}
