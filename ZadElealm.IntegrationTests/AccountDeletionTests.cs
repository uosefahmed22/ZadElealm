using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests;

public sealed class AccountDeletionTests : IClassFixture<ZadElealmApiFactory>
{
    private const string Email = "account-deletion@test.com";
    private readonly ZadElealmApiFactory _factory;

    public AccountDeletionTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteAccountRequiresOwnershipPassword_SoftDeletes_AndBlocksFutureLogin()
    {
        await EnsureUserAsync();

        using var anonymous = CreateClient();
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await anonymous.SendAsync(DeleteRequest("Password123!"))).StatusCode);

        using var student = CreateAuthenticatedClient();
        var missingPassword = await student.SendAsync(DeleteRequest(""));
        Assert.Equal(HttpStatusCode.BadRequest, missingPassword.StatusCode);

        var wrongPassword = await student.SendAsync(DeleteRequest("WrongPassword123!"));
        Assert.Equal(HttpStatusCode.BadRequest, wrongPassword.StatusCode);
        Assert.False(await IsSoftDeletedAsync());

        var deleted = await student.SendAsync(DeleteRequest(ZadElealmApiFactory.TestUserPassword));
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        Assert.True(await IsSoftDeletedAsync());

        var login = await anonymous.PostAsJsonAsync("/api/Account/login", new
        {
            email = Email,
            password = ZadElealmApiFactory.TestUserPassword
        });
        Assert.Equal(HttpStatusCode.NotFound, login.StatusCode);
    }

    private HttpRequestMessage DeleteRequest(string password)
    {
        return new HttpRequestMessage(HttpMethod.Delete, "/api/Account/delete-account")
        {
            Content = JsonContent.Create(new { password })
        };
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateTokenForEmail(Email, "User"));
        return client;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    private async Task EnsureUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        if (await userManager.FindByEmailAsync(Email) != null) return;

        var user = new AppUser
        {
            DisplayName = "طالب حذف الحساب",
            Email = Email,
            UserName = Email,
            EmailConfirmed = true
        };
        Assert.True((await userManager.CreateAsync(
            user,
            ZadElealmApiFactory.TestUserPassword)).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(user, "User")).Succeeded);
    }

    private async Task<bool> IsSoftDeletedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Users
            .IgnoreQueryFilters()
            .Where(user => user.Email == Email)
            .Select(user => user.IsDeleted)
            .SingleAsync();
    }
}
