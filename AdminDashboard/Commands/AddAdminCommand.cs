using AdminDashboard.Dto;
using MediatR;

namespace AdminDashboard.Commands
{
    public record AddAdminCommand : IRequest<AddAdminResult>
    {
        public string DisplayName { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
    }
}
