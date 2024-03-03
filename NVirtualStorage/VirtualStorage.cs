using Microsoft.Extensions.Options;
using NVirtualStorage.S3;

namespace NVirtualStorage;

public class VirtualStorage : IStorage
{
    Dictionary<string, StorageMount> _storages = new();

    public VirtualStorage(IOptions<VirtualStorageRootConfig> config)
    {
        if (config.Value.Mounts is not null)
        {
            var credentials = config.Value.Credentials.ToDictionary(i => i.Id);

            foreach (var storage in config.Value.Mounts)
            {
                StorageCredentials? credential = null;
                if (storage.CredentialsId.HasValue() &&
                    !credentials.TryGetValue(storage.CredentialsId!, out credential))
                {
                    throw new Exception($"Missing credentials {storage.CredentialsId} for storage {storage.Key}");
                }

                _storages.Add(storage.Key, new StorageMount { Config = storage, Credentials = credential });
            }
        }
    }


    public Task<VSFileInfo> UploadFileAsync(Stream stream, string fileName, string contentType = null)
    {
        var pathInfo = ParsePath(fileName);
        return pathInfo.Storage.GetStorage().UploadFileAsync(stream, pathInfo.InnerPath, contentType);
    }

    private PathInfo ParsePath(string fileName)
    {
        var pathSegments = fileName.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments.Length < 2)
        {
            throw new Exception($"Invalid path {fileName}");
        }

        var storageKey = pathSegments[0];
        if (!_storages.TryGetValue(storageKey, out var storage))
        {
            throw new Exception($"Unknown mount point {storageKey}");
        }

        return new PathInfo(storage, string.Join('/', pathSegments.Skip(1)));
    }

    public Task DeleteFileAsync(string fileName)
    {
        var pathInfo = ParsePath(fileName);
        return pathInfo.Storage.GetStorage().DeleteFileAsync(pathInfo.InnerPath);
    }

    public Task<string> GetPublicUrl(string fileName, TimeSpan? validity = null)
    {
        var pathInfo = ParsePath(fileName);
        return pathInfo.Storage.GetStorage().GetPublicUrl(pathInfo.InnerPath, validity);
    }

    public Task<VsFileContent?> LoadFileContentsAsync(string fileName)
    {
        var pathInfo = ParsePath(fileName);
        return pathInfo.Storage.GetStorage().LoadFileContentsAsync(pathInfo.InnerPath);
    }

    class StorageMount
    {
        public IStorage? Storage { get; set; }
        public StorageMounts Config { get; set; }
        public StorageCredentials? Credentials { get; set; }

        public IStorage GetStorage()
        {
            if (Storage is not null)
            {
                return Storage;
            }

            if (Config.Provider == StorageProviders.S3)
            {
                Storage = new S3Storage(Config, Credentials!);
                return Storage;
            }

            throw new Exception($"Unknown storage type {Config.Provider}");
        }
    }

    record PathInfo(StorageMount Storage, string InnerPath);
}