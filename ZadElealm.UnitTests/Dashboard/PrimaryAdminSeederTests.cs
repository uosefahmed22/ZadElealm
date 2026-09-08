using AdminDashboard.Helpers;
using AdminDashboard.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.UnitTests.Dashboard;

public class PrimaryAdminSeederTests
{
    [Fact]
    public async Task SeedAsync_WhenDisabled_DoesNotAccessIdentityStores()
    {
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var seeder = CreateSeeder(userManager.Object, roleManager.Object, enabled: false);

        await seeder.SeedAsync();

        userManager.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        roleManager.Verify(manager => manager.RoleExistsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsFresh_CreatesRoleAccountAndAssignment()
    {
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        AppUser? createdUser = null;

        roleManager.Setup(manager => manager.RoleExistsAsync("Admin")).ReturnsAsync(false);
        roleManager.Setup(manager => manager.CreateAsync(It.Is<IdentityRole>(role => role.Name == "Admin")))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.FindByEmailAsync("primary@test.com"))
            .ReturnsAsync((AppUser)null!);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<AppUser>(), "StrongPassword123!"))
            .Callback<AppUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.IsInRoleAsync(It.IsAny<AppUser>(), "Admin"))
            .ReturnsAsync(false);
        userManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<AppUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        var seeder = CreateSeeder(userManager.Object, roleManager.Object);

        await seeder.SeedAsync();

        Assert.NotNull(createdUser);
        Assert.Equal("primary@test.com", createdUser.Email);
        Assert.Equal("primary@test.com", createdUser.UserName);
        Assert.Equal("Primary Admin", createdUser.DisplayName);
        Assert.True(createdUser.EmailConfirmed);
        roleManager.Verify(
            manager => manager.CreateAsync(It.Is<IdentityRole>(role => role.Name == "Admin")),
            Times.Once);
        userManager.Verify(manager => manager.CreateAsync(createdUser, "StrongPassword123!"), Times.Once);
        userManager.Verify(manager => manager.AddToRoleAsync(createdUser, "Admin"), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WhenAdminAlreadyExists_DoesNotRecreateOrResetPassword()
    {
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var existingAdmin = new AppUser
        {
            Id = "admin-1",
            Email = "primary@test.com",
            UserName = "primary@test.com",
            DisplayName = "Existing Admin"
        };

        roleManager.Setup(manager => manager.RoleExistsAsync("Admin")).ReturnsAsync(true);
        userManager.Setup(manager => manager.FindByEmailAsync("primary@test.com"))
            .ReturnsAsync(existingAdmin);
        userManager.Setup(manager => manager.IsInRoleAsync(existingAdmin, "Admin"))
            .ReturnsAsync(true);

        var seeder = CreateSeeder(userManager.Object, roleManager.Object);

        await seeder.SeedAsync();

        userManager.Verify(
            manager => manager.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
            Times.Never);
        userManager.Verify(
            manager => manager.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SeedAsync_WhenEnabledWithoutPassword_FailsBeforeAccessingIdentityStores()
    {
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var seeder = CreateSeeder(userManager.Object, roleManager.Object, password: string.Empty);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.SeedAsync());

        Assert.Contains("AdminBootstrap:Password", exception.Message);
        userManager.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        roleManager.Verify(manager => manager.RoleExistsAsync(It.IsAny<string>()), Times.Never);
    }

    private static PrimaryAdminSeeder CreateSeeder(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        bool enabled = true,
        string password = "StrongPassword123!")
    {
        return new PrimaryAdminSeeder(
            userManager,
            roleManager,
            Options.Create(new AdminSettings
            {
                PrimaryAdminEmail = "primary@test.com",
                MaxAdminCount = 10
            }),
            Options.Create(new AdminBootstrapOptions
            {
                Enabled = enabled,
                DisplayName = "Primary Admin",
                Password = password
            }),
            Mock.Of<ILogger<PrimaryAdminSeeder>>());
    }

    private static Mock<UserManager<AppUser>> CreateUserManagerMock()
    {
        var userManager = new Mock<UserManager<AppUser>>(
            new Mock<IUserStore<AppUser>>().Object,
            null, null, null, null, null, null, null, null);
        userManager.Invocations.Clear();
        return userManager;
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
    {
        var roleManager = new Mock<RoleManager<IdentityRole>>(
            new Mock<IRoleStore<IdentityRole>>().Object,
            null, null, null, null);
        roleManager.Invocations.Clear();
        return roleManager;
    }
}
