using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using ZadElealm.Core.Models.Identity;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZadElealm.UnitTests.Dashboard;

namespace ZadElealm.UnitTests.Dashboard
{
    public class DeleteRoleCommandHandlerTests : DashboardTestBase
    {
        private RoleManager<IdentityRole> CreateRoleManager()
            => new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(DbContext), null, null, null, null);

        private async Task<IdentityRole> SeedRoleAsync(string name)
        {
            var role = new IdentityRole(name);
            DbContext.Roles.Add(role);
            await DbContext.SaveChangesAsync();
            return role;
        }

        [Fact]
        public async Task Handle_WhenRoleNotFound_Returns404()
        {
            var handler = new AdminDashboard.Handlers.DeleteRoleCommandHandler(CreateRoleManager());

            var result = await handler.Handle(
                new AdminDashboard.Commands.RoleCommand.DeleteRoleCommand { Id = "missing-id" },
                CancellationToken.None);

            Assert.Equal(404, result.StatusCode);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("User")]
        public async Task Handle_WhenRoleIsProtected_Returns400(string protectedName)
        {
            var role = await SeedRoleAsync(protectedName);
            var handler = new AdminDashboard.Handlers.DeleteRoleCommandHandler(CreateRoleManager());

            var result = await handler.Handle(
                new AdminDashboard.Commands.RoleCommand.DeleteRoleCommand { Id = role.Id },
                CancellationToken.None);

            Assert.Equal(400, result.StatusCode);
            Assert.True(DbContext.Roles.Any(r => r.Name == protectedName));
        }

        [Fact]
        public async Task Handle_WhenValidRequest_DeletesRoleAndReturns200()
        {
            var role = await SeedRoleAsync("Temporary");
            var handler = new AdminDashboard.Handlers.DeleteRoleCommandHandler(CreateRoleManager());

            var result = await handler.Handle(
                new AdminDashboard.Commands.RoleCommand.DeleteRoleCommand { Id = role.Id },
                CancellationToken.None);

            Assert.Equal(200, result.StatusCode);
            Assert.False(DbContext.Roles.Any(r => r.Id == role.Id));
        }
    }
}
