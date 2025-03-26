using AdminDashboard.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.UserCommand
{
    public class UpdateUserRolesCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsDeleted { get; set; }
        public List<RoleViewModel> Roles { get; set; }
    }
}
