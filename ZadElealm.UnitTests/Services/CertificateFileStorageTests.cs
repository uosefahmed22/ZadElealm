using Xunit;
using ZadElealm.Core.Options;
using ZadElealm.Service.Documents;

namespace ZadElealm.UnitTests.Services;

public sealed class CertificateFileStorageTests
{
    [Fact]
    public void RelativePaths_AreResolvedAgainstTheConfiguredContentRoot()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"zadelealm-{Guid.NewGuid():N}");
        var storage = new CertificateFileStorage(contentRoot, new CertificateStorageOptions
        {
            PrivatePath = Path.Combine("private", "certificates"),
            LegacyPublicPath = Path.Combine("legacy", "certificates"),
            LogoPath = Path.Combine("assets", "logo.png")
        });

        Assert.Equal(
            Path.GetFullPath(Path.Combine(contentRoot, "private", "certificates")),
            storage.GetPrivateDirectory());
        Assert.Equal(
            Path.GetFullPath(Path.Combine(contentRoot, "private", "certificates", "test.pdf")),
            storage.GetPrivateFilePath("test.pdf"));
        Assert.Equal(
            Path.GetFullPath(Path.Combine(contentRoot, "assets", "logo.png")),
            storage.GetLogoFilePath());
    }
}
