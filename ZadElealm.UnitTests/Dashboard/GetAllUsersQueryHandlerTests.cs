using AdminDashboard.Handlers.UserHandler;
using AdminDashboard.Quires.UserQuery;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Core.Models.Identity;
using Xunit;

namespace ZadElealm.UnitTests.Dashboard;

public sealed class GetAllUsersQueryHandlerTests : DashboardTestBase
{
    [Fact]
    public async Task Handle_MapsRolesForEveryUser()
    {
        var admin = new AppUser
        {
            Id = "admin-1",
            UserName = "admin@test.com",
            Email = "admin@test.com",
            DisplayName = "Admin"
        };
        var learner = new AppUser
        {
            Id = "user-1",
            UserName = "user@test.com",
            Email = "user@test.com",
            DisplayName = "Learner"
        };
        var deletedLearner = new AppUser
        {
            Id = "user-2",
            UserName = "deleted@test.com",
            Email = "deleted@test.com",
            DisplayName = "Deleted learner",
            IsDeleted = true
        };
        var adminRole = new IdentityRole("Admin") { Id = "role-admin" };
        var userRole = new IdentityRole("User") { Id = "role-user" };

        DbContext.Users.AddRange(admin, learner, deletedLearner);
        DbContext.Roles.AddRange(adminRole, userRole);
        DbContext.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = admin.Id, RoleId = adminRole.Id },
            new IdentityUserRole<string> { UserId = learner.Id, RoleId = userRole.Id });
        await DbContext.SaveChangesAsync();

        var handler = new GetAllUsersQueryHandler(DbContext);

        var result = (await handler.Handle(new GetAllUsersQuery(), CancellationToken.None)).ToList();

        Assert.Contains(result, item => item.Id == admin.Id && item.Roles.SequenceEqual(["Admin"]));
        Assert.Contains(result, item => item.Id == learner.Id && item.Roles.SequenceEqual(["User"]));
        Assert.Contains(result, item => item.Id == deletedLearner.Id && item.IsDeleted);
    }
}
