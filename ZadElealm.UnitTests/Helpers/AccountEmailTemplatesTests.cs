using Xunit;
using ZadElealm.Apis.Helpers;

namespace ZadElealm.UnitTests.Helpers;

public sealed class AccountEmailTemplatesTests
{
    [Fact]
    public void ChangeEmailOtp_BuildsSeparatedRtlOtpMessage()
    {
        var body = AccountEmailTemplates.ChangeEmailOtp("227110");

        Assert.Contains("lang=\"ar\" dir=\"rtl\"", body);
        Assert.Contains(">٢٢٧١١٠</div>", body);
        Assert.Contains("تأكيد تغيير البريد الإلكتروني", body);
        Assert.DoesNotContain("٢٢٧١١٠إذا", body);
    }

    [Fact]
    public void ConfirmationEmail_EncodesDynamicNameAndUrl()
    {
        var body = AccountEmailTemplates.WelcomeConfirmation(
            "<script>alert(1)</script>",
            "https://example.test/confirm?token=a&user=1");

        Assert.DoesNotContain("<script>", body);
        Assert.Contains("&lt;script&gt;", body);
        Assert.Contains("token=a&amp;user=1", body);
        Assert.Contains("تأكيد البريد الإلكتروني", body);
    }
}
