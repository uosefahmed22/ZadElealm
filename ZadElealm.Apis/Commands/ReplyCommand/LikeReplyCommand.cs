using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.ReplyCommand
{
    public class LikeReplyCommand : BaseCommand<ApiResponse>
    {
        public int ReplyId { get; set; }
        public string AppUserId { get; set; }
        public LikeReplyCommand(int replyId, string appUserId)
        {
            ReplyId = replyId;
            AppUserId = appUserId;
        }
    }
}
