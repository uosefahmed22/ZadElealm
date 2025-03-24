using MediatR;

namespace ZadElealm.Apis.Commands.UserRankCommand
{
    public class UpdateUserRankCommand : IRequest<bool>
    {
        public string UserId { get; set; }
    }
}
