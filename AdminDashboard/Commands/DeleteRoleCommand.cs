using AdminDashboard.Middlwares;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AdminDashboard.Commands
{
    public class DeleteRoleCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }
}