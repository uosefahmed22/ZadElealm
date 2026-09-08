using ZadElealm.Core.Options;
using ZadElealm.Core.Service;

namespace ZadElealm.Service.Documents;

public sealed class CertificateFileStorage : ICertificateFileStorage
{
    private const string PdfExtension = ".pdf";
    private readonly string _privateDirectory;
    private readonly string _legacyPublicDirectory;
    private readonly string _logoFilePath;

    public CertificateFileStorage(
        string contentRootPath,
        CertificateStorageOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);
        ArgumentNullException.ThrowIfNull(options);

        _privateDirectory = ResolveConfiguredPath(contentRootPath, options.PrivatePath);
        _legacyPublicDirectory = ResolveConfiguredPath(contentRootPath, options.LegacyPublicPath);
        _logoFilePath = ResolveConfiguredPath(contentRootPath, options.LogoPath);
    }

    public string GetPrivateDirectory() => _privateDirectory;

    public string GetPrivateFilePath(string fileName)
        => Path.Combine(_privateDirectory, GetSafePdfFileName(fileName));

    public string GetLogoFilePath() => _logoFilePath;

    public string? ResolveExistingFile(string storedReference)
    {
        var fileName = TryGetSafePdfFileName(storedReference);
        if (fileName is null)
            return null;

        var candidates = new[]
        {
            Path.Combine(_privateDirectory, fileName),
            Path.Combine(_legacyPublicDirectory, fileName)
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    public bool DeletePrivateFileIfExists(string? storedReference)
    {
        var fileName = TryGetSafePdfFileName(storedReference);
        if (fileName is null)
            return false;

        var filePath = Path.Combine(_privateDirectory, fileName);
        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);
        return true;
    }

    private static string GetSafePdfFileName(string value)
        => TryGetSafePdfFileName(value)
           ?? throw new ArgumentException("A valid PDF file name is required.", nameof(value));

    private static string? TryGetSafePdfFileName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var path = Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? Uri.UnescapeDataString(uri.AbsolutePath)
            : value;
        var fileName = Path.GetFileName(path.Replace('\\', '/'));

        return string.IsNullOrWhiteSpace(fileName) ||
               !fileName.EndsWith(PdfExtension, StringComparison.OrdinalIgnoreCase)
            ? null
            : fileName;
    }

    private static string ResolveConfiguredPath(string contentRootPath, string configuredPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuredPath);
        var path = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath);
        return Path.GetFullPath(path);
    }
}
