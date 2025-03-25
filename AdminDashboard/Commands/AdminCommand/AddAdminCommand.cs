using AdminDashboard.Dto;
using MediatR;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.AdminCommand
{
    public record AddAdminCommand : IRequest<ApiResponse>
    {
        public string DisplayName { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
    }
}
