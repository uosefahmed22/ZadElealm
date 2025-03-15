using AdminDashboard.Middlwares;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Commands
{
    public class CreateRoleCommand : IRequest<ApiResponse>
    {
        public string Name { get; set; }
    }
}
