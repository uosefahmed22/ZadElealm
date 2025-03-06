using MediatR;

namespace ZadElealm.Apis.Quaries
{
    public abstract class BaseQuery<TResponse> : IRequest<TResponse>
    {
    }
}
