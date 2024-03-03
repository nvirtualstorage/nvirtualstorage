namespace NVirtualStorage;

public class VirtualStorageRootConfig
{
    public StorageCredentials[] Credentials { get; set; }
    public StorageMounts[] Mounts { get; set; }
}

public class StorageMounts
{
    public string Key { get; set; }
    public StorageProviders Provider { get; set; }
    public string? CredentialsId { get; set; }
    public string? BasePath { get; set; }
    public string Bucket { get; set; }
    public string? Region { get; set; }
}

public class StorageCredentials
{
    public string Id { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string? Region { get; set; }
    public string? Url { get; set; }
}

public enum StorageProviders { S3 }