using AdminDashboard.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.UserCommand
{
    public class UpdateUserRolesCommand : IRequest<ApiResponse>
    {
        public UserRolesViewModel Model { get; set; }
    }
}
