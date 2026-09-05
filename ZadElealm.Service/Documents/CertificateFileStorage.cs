namespace ZadElealm.Service.Documents;

public static class CertificateFileStorage
{
    private const string PdfExtension = ".pdf";

    public static string GetPrivateDirectory()
        => Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "certificates");

    public static string GetPrivateFilePath(string fileName)
        => Path.Combine(GetPrivateDirectory(), GetSafePdfFileName(fileName));

    public static string? ResolveExistingFile(string storedReference)
    {
        var fileName = TryGetSafePdfFileName(storedReference);
        if (fileName is null)
            return null;

        var candidates = new[]
        {
            Path.Combine(GetPrivateDirectory(), fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates", fileName)
        };

        return candidates.FirstOrDefault(File.Exists);
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
}
