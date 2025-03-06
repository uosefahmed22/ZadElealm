using MediatR;
using ZadElealm.Apis.Quaries;

namespace ZadElealm.Apis.Handlers
{
    public abstract class BaseQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : BaseQuery<TResponse>
    {
        public abstract Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken);
    }
}
