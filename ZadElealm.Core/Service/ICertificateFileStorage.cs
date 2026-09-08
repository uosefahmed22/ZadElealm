namespace ZadElealm.Core.Service;

public interface ICertificateFileStorage
{
    string GetPrivateDirectory();
    string GetPrivateFilePath(string fileName);
    string GetLogoFilePath();
    string? ResolveExistingFile(string storedReference);
    bool DeletePrivateFileIfExists(string? storedReference);
}
