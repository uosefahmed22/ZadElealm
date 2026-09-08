namespace AdminDashboard.Helpers;

public sealed class AdminBootstrapOptions
{
    public const string SectionName = "AdminBootstrap";

    public bool Enabled { get; set; }
    public string DisplayName { get; set; } = "Primary Admin";
    public string Password { get; set; } = string.Empty;
}
