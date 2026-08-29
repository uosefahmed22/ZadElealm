
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.Review
{
    public class ToggleLikeCommand : BaseCommand<ApiResponse>
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }
    }
}
