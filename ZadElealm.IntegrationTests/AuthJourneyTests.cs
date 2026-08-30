using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ZadElealm.IntegrationTests;

public class AuthJourneyTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly ZadElealmApiFactory _factory;

    public AuthJourneyTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_CurrentUser_AndRefreshToken_CompleteSuccessfully()
    {
        using var client = CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/Account/login", new
        {
            email = "user@test.com",
            password = ZadElealmApiFactory.TestUserPassword
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await ReadAuthResponse(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
        Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        var currentUserResponse = await client.GetAsync("/api/Account/current-user");

        Assert.Equal(HttpStatusCode.OK, currentUserResponse.StatusCode);
        var currentUser = await ReadAuthResponse(currentUserResponse);
        Assert.Equal("user@test.com", currentUser.Email);

        var refreshResponse = await client.PostAsJsonAsync("/api/Account/refresh-token", new
        {
            token = login.Token,
            refreshToken = login.RefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshed = await ReadAuthResponse(refreshResponse);
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
        Assert.False(string.IsNullOrWhiteSpace(refreshed.Token));
    }

    [Fact]
    public async Task CurrentUser_WithoutToken_IsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/Account/current-user");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidInput_ReturnsSuccessWithoutExternalEmail()
    {
        using var client = CreateClient();
        var email = $"new-{Guid.NewGuid():N}@test.com";

        var response = await client.PostAsJsonAsync("/api/Account/register", new
        {
            displayName = "طالب جديد",
            email,
            password = "Registration123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static async Task<AuthData> ReadAuthResponse(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");

        return new AuthData(
            data.GetProperty("email").GetString() ?? string.Empty,
            data.GetProperty("token").GetString() ?? string.Empty,
            data.GetProperty("refreshToken").GetString() ?? string.Empty);
    }

    private sealed record AuthData(string Email, string Token, string RefreshToken);
}
