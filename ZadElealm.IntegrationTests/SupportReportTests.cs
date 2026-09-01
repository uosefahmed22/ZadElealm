using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Enums;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests;

public sealed class SupportReportTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly ZadElealmApiFactory _factory;

    public SupportReportTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UserCanCreateSafeSupportReport_AndCannotSetAdminOwnedState()
    {
        using var client = CreateAuthenticatedClient();
        var title = $"مشكلة تشغيل {Guid.NewGuid():N}";

        var invalidType = await client.PostAsJsonAsync("/api/Report", new
        {
            titleOfTheIssue = title,
            description = "هذا وصف كاف للمشكلة التي ظهرت أثناء التشغيل",
            reportType = "NotARealType"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidType.StatusCode);

        var response = await client.PostAsJsonAsync("/api/Report", new
        {
            titleOfTheIssue = title,
            description = "  هذا وصف كاف للمشكلة التي ظهرت أثناء تشغيل الدرس  ",
            reportType = "Technical",
            adminResponse = "تم الحل من الطالب",
            isSolved = true,
            id = 999
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var report = await dbContext.Reports.SingleAsync(item => item.TitleOfTheIssue == title);
        Assert.Equal(ReportType.Technical, report.reportTypes);
        Assert.Equal("هذا وصف كاف للمشكلة التي ظهرت أثناء تشغيل الدرس", report.Description);
        Assert.Null(report.AdminResponse);
        Assert.False(report.IsSolved);
        Assert.NotEqual(999, report.Id);
    }

    [Fact]
    public async Task ReportEndpoint_ValidatesInputAndRequiresUserAuthentication()
    {
        using var anonymous = CreateClient();
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await anonymous.PostAsJsonAsync("/api/Report", new
            {
                titleOfTheIssue = "مشكلة",
                description = "هذا وصف كاف للمشكلة",
                reportType = "Other"
            })).StatusCode);

        using var user = CreateAuthenticatedClient();
        var invalid = await user.PostAsJsonAsync("/api/Report", new
        {
            titleOfTheIssue = "",
            description = "قصير",
            reportType = "Other"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var authenticatedUser = CreateAuthenticatedClient();
        Assert.Equal(HttpStatusCode.NotFound, (await authenticatedUser.GetAsync("/api/Report/mine")).StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string email = "user@test.com")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateTokenForEmail(email, "User"));
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

}
