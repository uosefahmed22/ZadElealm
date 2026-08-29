using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.Review
{
    public class AddReplyCommand : BaseCommand<ApiResponse>
    {
        public string ReplyText { get; set; }
        public int ReviewId { get; set; }
        public string UserId { get; set; }
    }
}
