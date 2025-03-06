using MediatR;
using ZadElealm.Apis.Commands;

namespace ZadElealm.Apis.Handlers
{
    public abstract class BaseCommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
     where TCommand : BaseCommand<TResponse>
    {
        public abstract Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);
    }
}
