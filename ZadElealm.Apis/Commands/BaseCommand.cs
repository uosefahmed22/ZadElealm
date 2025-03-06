using MediatR;

namespace ZadElealm.Apis.Commands
{
    public abstract class BaseCommand<TResponse> : IRequest<TResponse>
    {
    }
}
