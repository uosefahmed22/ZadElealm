using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.VideoProgressCommands
{
    public class UpdateVideoProgressCommand : BaseCommand<ApiDataResponse>
    {
        public string UserId { get; set; }
        public int VideoId { get; set; }
        public TimeSpan WatchedDuration { get; set; }
    }
}
