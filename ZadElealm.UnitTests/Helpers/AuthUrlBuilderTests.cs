using Xunit;
using ZadElealm.Apis.Helpers;

namespace ZadElealm.UnitTests.Helpers
{
    public class AuthUrlBuilderTests
    {
        [Fact]
        public void BuildConfirmEmailUrl_TrimsTrailingSlashAndBuildsExpectedPath()
        {
            var result = AuthUrlBuilder.BuildConfirmEmailUrl(
                "https://zadelealm.runasp.net/",
                "user-123",
                "encoded-token");

            Assert.Equal(
                "https://zadelealm.runasp.net/api/Account/confirm-email?userId=user-123&token=encoded-token",
                result);
        }

        [Fact]
        public void BuildFrontendLoginUrl_TrimsTrailingSlashAndTargetsLoginPage()
        {
            var result = AuthUrlBuilder.BuildFrontendLoginUrl("https://zad-elealm.netlify.app/");

            Assert.Equal("https://zad-elealm.netlify.app/login", result);
        }
    }
}
