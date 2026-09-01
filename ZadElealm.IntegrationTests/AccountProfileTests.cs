using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.IntegrationTests;

public sealed class AccountProfileTests : IClassFixture<ZadElealmApiFactory>
{
    private const string OriginalEmail = "account-profile@test.com";
    private const string UpdatedEmail = "account-profile-updated@test.com";
    private const string OriginalPassword = ZadElealmApiFactory.TestUserPassword;
    private const string UpdatedPassword = "UpdatedPassword123!";
    private readonly ZadElealmApiFactory _factory;

    public AccountProfileTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UserCanUpdateProfileImagePasswordAndEmailThroughOwnedAccountFlow()
    {
        await EnsureAccountUserAsync();
        using var client = CreateAuthenticatedClient(OriginalEmail);

        var initial = await ReadProfileAsync(client);
        Assert.Equal("Account Profile Test", initial.DisplayName);

        var invalidName = await client.PostAsJsonAsync(
            "/api/Account/update-profile",
            new { displayName = "English Name", phoneNumber = "+201001234567" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidName.StatusCode);

        var updateProfile = await client.PostAsJsonAsync(
            "/api/Account/update-profile",
            new { displayName = "يوسف أحمد", phoneNumber = "+201001234567" });
        Assert.Equal(HttpStatusCode.OK, updateProfile.StatusCode);
        var updatedProfile = await ReadProfileAsync(client);
        Assert.Equal("يوسف أحمد", updatedProfile.DisplayName);
        Assert.Equal("+201001234567", updatedProfile.PhoneNumber);

        using var imageContent = new MultipartFormDataContent();
        imageContent.Add(
            new ByteArrayContent([1, 2, 3, 4])
            {
                Headers = { ContentType = new MediaTypeHeaderValue("image/png") }
            },
            "file",
            "profile.png");
        var uploadImage = await client.PostAsync("/api/Account/update-profile-image", imageContent);
        Assert.Equal(HttpStatusCode.OK, uploadImage.StatusCode);
        Assert.Equal(
            "https://example.test/profile/integration-user.png",
            (await ReadProfileAsync(client)).ImageUrl);

        var deleteImage = await client.PostAsync("/api/Account/update-profile-image", null);
        Assert.Equal(HttpStatusCode.OK, deleteImage.StatusCode);
        Assert.Null((await ReadProfileAsync(client)).ImageUrl);

        var wrongCurrentPassword = await client.PostAsJsonAsync(
            "/api/Account/change-password",
            new { currentPassword = "WrongPassword", newPassword = UpdatedPassword });
        Assert.Equal(HttpStatusCode.BadRequest, wrongCurrentPassword.StatusCode);

        var changePassword = await client.PostAsJsonAsync(
            "/api/Account/change-password",
            new { currentPassword = OriginalPassword, newPassword = UpdatedPassword });
        Assert.Equal(HttpStatusCode.OK, changePassword.StatusCode);

        var oldPasswordLogin = await LoginAsync(OriginalEmail, OriginalPassword);
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLogin.StatusCode);
        var newPasswordLogin = await LoginAsync(OriginalEmail, UpdatedPassword);
        Assert.Equal(HttpStatusCode.OK, newPasswordLogin.StatusCode);

        var wrongPasswordOtp = await client.PostAsJsonAsync(
            "/api/Account/send-email-otp",
            new { newEmail = UpdatedEmail, password = "WrongPassword" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPasswordOtp.StatusCode);

        var sendOtp = await client.PostAsJsonAsync(
            "/api/Account/send-email-otp",
            new { newEmail = UpdatedEmail, password = UpdatedPassword });
        Assert.Equal(HttpStatusCode.OK, sendOtp.StatusCode);

        var wrongOtp = await client.PostAsJsonAsync(
            "/api/Account/update-email",
            new { newEmail = UpdatedEmail, token = "000000" });
        Assert.Equal(HttpStatusCode.BadRequest, wrongOtp.StatusCode);

        var updateEmail = await client.PostAsJsonAsync(
            "/api/Account/update-email",
            new { newEmail = UpdatedEmail, token = ZadElealmApiFactory.TestOtp });
        Assert.Equal(HttpStatusCode.OK, updateEmail.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await LoginAsync(OriginalEmail, UpdatedPassword)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(UpdatedEmail, UpdatedPassword)).StatusCode);
    }

    [Fact]
    public async Task AccountManagementEndpoints_RequireAuthentication()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Account/get-User-Profile")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/Account/update-profile", new { displayName = "يوسف" })).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync(
                "/api/Account/change-password",
                new { currentPassword = "password", newPassword = "password2" })).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsync("/api/Account/update-profile-image", null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync(
                "/api/Account/send-email-otp",
                new { newEmail = "new@test.com", password = "password" })).StatusCode);
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

    private async Task<HttpResponseMessage> LoginAsync(string email, string password)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        return await client.PostAsJsonAsync("/api/Account/login", new { email, password });
    }

    private async Task EnsureAccountUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        if (await userManager.FindByEmailAsync(OriginalEmail) != null) return;
        if (await userManager.FindByEmailAsync(UpdatedEmail) is { } oldUpdatedUser)
            await userManager.DeleteAsync(oldUpdatedUser);

        var user = new AppUser
        {
            DisplayName = "Account Profile Test",
            Email = OriginalEmail,
            UserName = OriginalEmail,
            EmailConfirmed = true
        };
        var created = await userManager.CreateAsync(user, OriginalPassword);
        Assert.True(created.Succeeded);
        var role = await userManager.AddToRoleAsync(user, "User");
        Assert.True(role.Succeeded);
    }

    private static async Task<UserProfile> ReadProfileAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/Account/get-User-Profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        return new UserProfile(
            data.GetProperty("displayName").GetString() ?? string.Empty,
            data.GetProperty("email").GetString() ?? string.Empty,
            data.GetProperty("phoneNumber").ValueKind == JsonValueKind.Null
                ? null
                : data.GetProperty("phoneNumber").GetString(),
            data.GetProperty("imageUrl").ValueKind == JsonValueKind.Null
                ? null
                : data.GetProperty("imageUrl").GetString());
    }

    private sealed record UserProfile(
        string DisplayName,
        string Email,
        string? PhoneNumber,
        string? ImageUrl);
}
