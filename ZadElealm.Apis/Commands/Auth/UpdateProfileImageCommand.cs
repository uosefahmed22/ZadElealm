using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries;

namespace ZadElealm.Apis.Commands.Auth
{
    public class UpdateProfileImageCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public IFormFile? File { get; }

        public UpdateProfileImageCommand(string userId, IFormFile? file)
        {
            UserId = userId;
            File = file;
        }
    }
}
