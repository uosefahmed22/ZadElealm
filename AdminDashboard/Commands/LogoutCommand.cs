using MediatR;

namespace AdminDashboard.Commands
{
    public record LogoutCommand : IRequest<Unit>;
}
