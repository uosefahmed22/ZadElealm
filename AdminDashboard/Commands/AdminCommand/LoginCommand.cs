using AdminDashboard.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Commands.AdminCommand
{
    public record LoginCommand : IRequest<LoginResult>
    {
        public string Email { get; init; }
        public string Password { get; init; }
    }
}
