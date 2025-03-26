using AdminDashboard.Models;
using AdminDashboard.Quires.UserQuery;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Handlers.UserHandler
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserViewModel>>
    {
        private readonly UserManager<AppUser> _userManager;

        public GetAllUsersQueryHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserViewModel>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userManager.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    IsDeleted = u.IsDeleted,
                    IsConfirmed = u.EmailConfirmed,
                    DisplayName = u.DisplayName,
                    Roles = new List<string>()
                }).ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                var userEntity = await _userManager.FindByIdAsync(user.Id);
                user.Roles = (await _userManager.GetRolesAsync(userEntity)).ToList();
            }
            return users;
        }
    }
}
