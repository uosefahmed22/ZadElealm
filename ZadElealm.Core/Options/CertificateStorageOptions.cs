namespace ZadElealm.Core.Options;

public sealed class CertificateStorageOptions
{
    public const string SectionName = "CertificateStorage";

    public string PrivatePath { get; set; } = Path.Combine("App_Data", "certificates");
    public string LegacyPublicPath { get; set; } = Path.Combine("wwwroot", "certificates");
    public string LogoPath { get; set; } = Path.Combine("wwwroot", "certificates", "logo.png");
}
