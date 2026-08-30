using AdminDashboard.Handlers.UserHandler;
using AdminDashboard.Quires.UserQuery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ZadElealm.Core.Models.Identity;
using Xunit;

namespace ZadElealm.UnitTests.Dashboard;

public sealed class GetUserForEditQueryHandlerTests : DashboardTestBase
{
    [Fact]
    public async Task Handle_LoadsRolesSequentiallyOnTheScopedDbContext()
    {
        DbContext.Roles.AddRange(
            new IdentityRole("Admin"),
            new IdentityRole("User"),
            new IdentityRole("Instructor"));
        await DbContext.SaveChangesAsync();

        var user = new AppUser
        {
            Id = "user-1",
            DisplayName = "Test User",
            EmailConfirmed = true
        };
        var activeOperations = 0;
        var maximumConcurrency = 0;
        var concurrencyLock = new object();
        var userManager = CreateUserManagerMock();
        userManager.Setup(manager => manager.FindByIdAsync(user.Id)).ReturnsAsync(user);
        userManager
            .Setup(manager => manager.IsInRoleAsync(user, It.IsAny<string>()))
            .Returns(async (AppUser _, string roleName) =>
            {
                var currentOperations = Interlocked.Increment(ref activeOperations);
                lock (concurrencyLock)
                {
                    maximumConcurrency = Math.Max(maximumConcurrency, currentOperations);
                }

                await Task.Delay(20);
                Interlocked.Decrement(ref activeOperations);
                return roleName == "User";
            });

        var roleStore = new RoleStore<IdentityRole, ZadElealm.Repository.Data.Datbases.AppDbContext>(DbContext);
        var roleManager = new RoleManager<IdentityRole>(
            roleStore,
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<IdentityRole>>.Instance);
        var handler = new GetUserForEditQueryHandler(userManager.Object, roleManager);

        var result = await handler.Handle(
            new GetUserForEditQuery { UserId = user.Id },
            CancellationToken.None);

        Assert.Equal(3, result.Roles.Count);
        Assert.True(result.IsConfirmed);
        Assert.False(result.Roles.Single(role => role.Name == "Admin").IsSelected);
        Assert.True(result.Roles.Single(role => role.Name == "User").IsSelected);
        Assert.False(result.Roles.Single(role => role.Name == "Instructor").IsSelected);
        Assert.Equal(1, maximumConcurrency);
    }

    private static Mock<UserManager<AppUser>> CreateUserManagerMock()
        => new(
            new Mock<IUserStore<AppUser>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
}
