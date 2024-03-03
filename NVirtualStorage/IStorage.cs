namespace NVirtualStorage;

public interface IStorage
{
    Task<VSFileInfo> UploadFileAsync(Stream stream, string fileName, string contentType = null);
    Task DeleteFileAsync(string fileName);
    Task<string> GetPublicUrl(string fileName, TimeSpan? validity);

    public Task<VsFileContent?> LoadFileContentsAsync(string fileName);
}