using AdminDashboard.Middlwares;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Commands.RoleCommand
{
    public class CreateRoleCommand : IRequest<ApiResponse>
    {
        public string Name { get; set; }
    }
}
