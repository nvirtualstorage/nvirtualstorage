namespace NVirtualStorage;

public abstract class BaseStorage : IStorage
{
    public abstract Task<VSFileInfo> UploadFileAsync(Stream stream, string fileName, string contentType = null);
    public abstract Task DeleteFileAsync(string fileName);
    public abstract Task<string> GetPublicUrl(string fileName, TimeSpan? validity);
    public abstract Task<VsFileContent> LoadFileContentsAsync(string fileName);
}