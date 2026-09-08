using AdminDashboard.Models;
using AdminDashboard.Quires.UserQuery;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;

namespace AdminDashboard.Handlers.UserHandler
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserViewModel>>
    {
        private readonly AppDbContext _dbContext;

        public GetAllUsersQueryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserViewModel>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _dbContext.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
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

            var roleMemberships = await (
                from userRole in _dbContext.UserRoles
                join role in _dbContext.Roles on userRole.RoleId equals role.Id
                select new { userRole.UserId, RoleName = role.Name! })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var rolesByUser = roleMemberships
                .GroupBy(item => item.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(item => item.RoleName).ToList());

            foreach (var user in users)
            {
                if (rolesByUser.TryGetValue(user.Id, out var roles))
                    user.Roles = roles;
            }

            return users;
        }
    }
}
