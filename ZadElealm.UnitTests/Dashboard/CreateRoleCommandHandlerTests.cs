using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using ZadElealm.Core.Models.Identity;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZadElealm.UnitTests.Dashboard;

namespace ZadElealm.UnitTests.Dashboard
{
    public class CreateRoleCommandHandlerTests : DashboardTestBase
    {
        private RoleManager<IdentityRole> CreateRoleManager()
            => new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(DbContext), null, null, null, null);

        [Fact]
        public async Task Handle_WhenPrimaryAdminNotConfirmed_Returns400()
        {
            var userManager = CreateUserManagerMock(
                new AppUser { Id = "admin-1", Email = "primary@test.com" }, emailConfirmed: false);
            var handler = new AdminDashboard.Handlers.CreateRoleCommandHandler(
                CreateRoleManager(), userManager, BuildConfiguration());

            var result = await handler.Handle(
                new AdminDashboard.Commands.RoleCommand.CreateRoleCommand { Name = "Instructor" },
                CancellationToken.None);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Handle_WhenMaxRolesReached_Returns400()
        {
            DbContext.Roles.Add(new IdentityRole("ExistingRole"));
            DbContext.SaveChanges();

            var userManager = CreateUserManagerMock(
                new AppUser { Id = "admin-1", Email = "primary@test.com", EmailConfirmed = true });
            var handler = new AdminDashboard.Handlers.CreateRoleCommandHandler(
                CreateRoleManager(), userManager, BuildConfiguration(maxAdminCount: 1));

            var result = await handler.Handle(
                new AdminDashboard.Commands.RoleCommand.CreateRoleCommand { Name = "AnotherRole" },
                CancellationToken.None);

            Assert.Equal(400, result.StatusCode);
            Assert.Equal(1, DbContext.Roles.Count());
        }

        [Fact]
        public async Task Handle_WhenRoleExists_Returns400()
        {
            DbContext.Roles.Add(new IdentityRole("Instructor") { NormalizedName = "Instructor" });
            DbContext.SaveChanges();

            var userManager = CreateUserManagerMock(
                new AppUser { Id = "admin-1", Email = "primary@test.com", EmailConfirmed = true });
            var handler = new AdminDashboard.Handlers.CreateRoleCommandHandler(
                CreateRoleManager(), userManager, BuildConfiguration(maxAdminCount: 10));

            var result = await handler.Handle(
                new AdminDashboard.Commands.RoleCommand.CreateRoleCommand { Name = "Instructor" },
                CancellationToken.None);

            Assert.Equal(400, result.StatusCode);
            Assert.Equal(1, DbContext.Roles.Count());
        }

        [Fact]
        public async Task Handle_WhenValidRequest_CreatesRoleAndReturns200()
        {
            var userManager = CreateUserManagerMock(
                new AppUser { Id = "admin-1", Email = "primary@test.com", EmailConfirmed = true });
            var handler = new AdminDashboard.Handlers.CreateRoleCommandHandler(
                CreateRoleManager(), userManager, BuildConfiguration(maxAdminCount: 10));

            var result = await handler.Handle(
                new AdminDashboard.Commands.RoleCommand.CreateRoleCommand { Name = "Instructor" },
                CancellationToken.None);

            Assert.Equal(200, result.StatusCode);
            Assert.True(DbContext.Roles.Any(r => r.Name == "Instructor"));
        }
    }
}
