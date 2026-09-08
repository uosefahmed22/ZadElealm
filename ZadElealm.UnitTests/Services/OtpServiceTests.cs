using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZadElealm.Core.Localization;
using ZadElealm.Service.IdentityService;

namespace ZadElealm.UnitTests.Services;

public sealed class OtpServiceTests
{
    [Fact]
    public void IsValidOtp_AcceptsArabicDigits()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OtpSettings:Step"] = "30",
                ["OtpSettings:KeyExpirationMinutes"] = "5",
                ["OtpSettings:ValidationExpirationMinutes"] = "5",
            })
            .Build();
        var service = new OtpService(cache, NullLogger<OtpService>.Instance, configuration);
        var email = "arabic-otp@example.test";
        var otp = service.GenerateOtp(email);

        var result = service.IsValidOtp(email, ArabicNumerals.Localize(otp));

        Assert.True(result);
    }
}
