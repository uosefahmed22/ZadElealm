using AdminDashboard.Middlwares;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.RoleCommand
{
    public class DeleteRoleCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }
}