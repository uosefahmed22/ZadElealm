using ZadElealm.Apis.Dtos;

namespace ZadElealm.Apis.Commands.VideoProgressCommands
{
    public class UpdateVideoProgressCommand : BaseCommand<VideoProgressDto>
    {
        public string UserId { get; set; }
        public int VideoId { get; set; }
        public TimeSpan WatchedDuration { get; set; }
    }
}
