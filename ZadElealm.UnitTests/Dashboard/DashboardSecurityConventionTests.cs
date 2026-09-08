using System.Reflection;
using AdminDashboard.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Xunit;
using DashboardAdminController = AdminDashboard.Controllers.AdminDashboard.Controllers.AdminController;

namespace ZadElealm.UnitTests.Dashboard;

public sealed class DashboardSecurityConventionTests
{
    [Theory]
    [InlineData(typeof(UserController), "Delete")]
    [InlineData(typeof(RoleController), "Delete")]
    [InlineData(typeof(CategoryController), "Delete")]
    [InlineData(typeof(CourseController), "Delete")]
    [InlineData(typeof(DashboardAdminController), "Logout")]
    public void StateChangingActions_RequirePost(Type controllerType, string actionName)
    {
        var action = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == actionName);

        Assert.NotNull(action.GetCustomAttribute<HttpPostAttribute>());
    }

    [Fact]
    public void AddAdminGet_RequiresAdminRole()
    {
        var action = typeof(DashboardAdminController).GetMethods()
            .Single(method => method.Name == "AddAdmin" && method.GetCustomAttribute<HttpGetAttribute>() != null);

        var authorize = action.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);
        Assert.Equal("Admin", authorize!.Roles);
    }

    [Fact]
    public void LoginPost_HasDedicatedRateLimitPolicy()
    {
        var action = typeof(DashboardAdminController).GetMethods()
            .Single(method => method.Name == "Login" && method.GetCustomAttribute<HttpPostAttribute>() != null);

        var rateLimit = action.GetCustomAttribute<EnableRateLimitingAttribute>();
        Assert.NotNull(rateLimit);
        Assert.Equal("admin-login", rateLimit!.PolicyName);
    }
}
