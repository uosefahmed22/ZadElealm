using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests;

public sealed class NotificationCenterTests : IClassFixture<ZadElealmApiFactory>
{
    private const string OtherUserEmail = "notification-owner-check@test.com";
    private readonly ZadElealmApiFactory _factory;

    public NotificationCenterTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UserCanListAndManageOnlyOwnedNotifications_WithoutReadOnFetch()
    {
        await EnsureOtherUserAsync();
        var ownNotificationId = await AddNotificationAsync("user@test.com", "إشعار الطالب");
        var otherNotificationId = await AddNotificationAsync(OtherUserEmail, "إشعار مستخدم آخر");
        using var student = CreateAuthenticatedClient("user@test.com");

        var firstList = await ReadNotificationsAsync(student);
        var ownNotification = Assert.Single(firstList.Notifications);
        Assert.Equal(ownNotificationId, ownNotification.Id);
        Assert.False(ownNotification.IsRead);
        Assert.Equal(1, firstList.UnreadCount);

        var secondList = await ReadNotificationsAsync(student);
        Assert.Equal(1, secondList.UnreadCount);
        Assert.False(Assert.Single(secondList.Notifications).IsRead);

        var forbiddenRead = await student.PostAsync(
            $"/api/Notification/mark-as-read/{otherNotificationId}",
            null);
        var forbiddenDelete = await student.DeleteAsync($"/api/Notification/{otherNotificationId}");
        var forbiddenDetails = await student.GetAsync($"/api/Notification/{otherNotificationId}");
        Assert.Equal(HttpStatusCode.NotFound, forbiddenRead.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, forbiddenDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, forbiddenDetails.StatusCode);

        var markOne = await student.PostAsync(
            $"/api/Notification/mark-as-read/{ownNotificationId}",
            null);
        Assert.Equal(HttpStatusCode.OK, markOne.StatusCode);
        var afterMarkOne = await ReadNotificationsAsync(student);
        Assert.Equal(0, afterMarkOne.UnreadCount);
        Assert.True(Assert.Single(afterMarkOne.Notifications).IsRead);

        await AddNotificationAsync("user@test.com", "إشعار ثانٍ");
        var beforeMarkAll = await ReadNotificationsAsync(student);
        Assert.Equal(1, beforeMarkAll.UnreadCount);
        Assert.Equal(2, beforeMarkAll.TotalCount);

        var markAll = await student.PostAsync("/api/Notification/mark-all-as-read", null);
        Assert.Equal(HttpStatusCode.OK, markAll.StatusCode);
        Assert.Equal(0, (await ReadNotificationsAsync(student)).UnreadCount);

        var deleteOwn = await student.DeleteAsync($"/api/Notification/{ownNotificationId}");
        Assert.Equal(HttpStatusCode.OK, deleteOwn.StatusCode);
        var afterDelete = await ReadNotificationsAsync(student);
        Assert.Single(afterDelete.Notifications);
        Assert.Equal(1, afterDelete.TotalCount);
    }

    [Fact]
    public async Task NotificationEndpoints_RequireAuthentication()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Notification")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsync("/api/Notification/mark-all-as-read", null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsync("/api/Notification/mark-as-read/1", null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.DeleteAsync("/api/Notification/1")).StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string email)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateTokenForEmail(email, "User"));
        return client;
    }

    private async Task<int> AddNotificationAsync(string email, string title)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notification = new Notification
        {
            Title = title,
            Description = "وصف إشعار التكامل",
            Type = NotificationType.System,
            CreatedAt = DateTime.UtcNow,
            UserNotifications =
            [
                new UserNotification
                {
                    AppUserId = user.Id,
                    IsRead = false
                }
            ]
        };
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();
        return notification.Id;
    }

    private async Task EnsureOtherUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        if (await userManager.FindByEmailAsync(OtherUserEmail) != null) return;

        var user = new AppUser
        {
            DisplayName = "Notification Ownership Test",
            Email = OtherUserEmail,
            UserName = OtherUserEmail,
            EmailConfirmed = true
        };
        var created = await userManager.CreateAsync(user, ZadElealmApiFactory.TestUserPassword);
        Assert.True(created.Succeeded);
        var role = await userManager.AddToRoleAsync(user, "User");
        Assert.True(role.Succeeded);
    }

    private static async Task<NotificationList> ReadNotificationsAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/Notification");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var notifications = data.GetProperty("notifications")
            .EnumerateArray()
            .Select(item => new NotificationItem(
                item.GetProperty("id").GetInt32(),
                item.GetProperty("isRead").GetBoolean()))
            .ToList();
        return new NotificationList(
            notifications,
            data.GetProperty("unreadCount").GetInt32(),
            data.GetProperty("totalCount").GetInt32());
    }

    private sealed record NotificationItem(int Id, bool IsRead);
    private sealed record NotificationList(
        IReadOnlyList<NotificationItem> Notifications,
        int UnreadCount,
        int TotalCount);
}
