using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Review
{
    public class DeleteReplyreviewCommand : BaseCommand<ApiResponse>
    {
        public int ReplyId { get; set; }
        public string UserId { get; set; }
        public DeleteReplyreviewCommand(int replyId, string userId)
        {
            ReplyId = replyId;
            UserId = userId;
        }
    }
}
