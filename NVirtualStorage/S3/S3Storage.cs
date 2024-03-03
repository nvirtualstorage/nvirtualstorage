using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace NVirtualStorage.S3;

public class S3Storage : BaseStorage
{
    private readonly AmazonS3Config _s3Config;
    private readonly BasicAWSCredentials _awsCredentials;
    private readonly StorageMounts _config;
    private readonly StorageCredentials _credentials;

    public S3Storage(StorageMounts config, StorageCredentials credentials)
    {
        _awsCredentials = new BasicAWSCredentials(credentials.AccessKey, credentials.SecretKey);

        if (credentials.Url.HasValue())
        {
            _s3Config = new AmazonS3Config()
            {
                ServiceURL = credentials.Url,
                //RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region ?? credentials.Region),
                ForcePathStyle = true,
                UseHttp = !credentials.Url.Contains("https://"),
            };
        }
        else
        {
            _s3Config = new AmazonS3Config()
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region ?? credentials.Region),
            };
        }

        _credentials = credentials;
        _config = config;
    }

    public override async Task<VSFileInfo> UploadFileAsync(Stream stream, string fileName, string contentType = null)
    {
        var path = ProcessPath(fileName);
        using var client = new AmazonS3Client(_awsCredentials, _s3Config);
        var request = new PutObjectRequest
        {
            BucketName = _config.Bucket, Key = path, InputStream = stream, ContentType = contentType,
        };
        var response = await client.PutObjectAsync(request);

        return new VSFileInfo
        {
            Name = fileName,
            //Size = stream.Length,
            ContentType = contentType,
            InternalPath = $"https://{_config.Bucket}.s3.amazonaws.com/{path}",
        };
    }

    public override async Task<VsFileContent?> LoadFileContentsAsync(string fileName)
    {
        try
        {
            var path = ProcessPath(fileName);
            using var client = new AmazonS3Client(_awsCredentials, _s3Config);
            var request = new GetObjectRequest { BucketName = _config.Bucket, Key = path };
            var response = await client.GetObjectAsync(request);

            return new VsFileContent()
            {
                Name = fileName,
                LastUpdate = response.LastModified,
                //Size = response.ContentLength,
                Size = response.ContentLength,
                ContentType = null,
                Contents = response.ResponseStream
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            return null;
        }
    }

    public override async Task DeleteFileAsync(string fileName)
    {
        var path = ProcessPath(fileName);
        using var client = new AmazonS3Client(_awsCredentials, _s3Config);
        var request = new DeleteObjectRequest { BucketName = _config.Bucket, Key = path };
        await client.DeleteObjectAsync(request);
    }

    public override async Task<string> GetPublicUrl(string fileName, TimeSpan? validity)
    {
        var path = ProcessPath(fileName);
        using var client = new AmazonS3Client(_awsCredentials, _s3Config);

        var expiresAt = DateTime.UtcNow.Add(validity ?? new TimeSpan(1, 0, 0));

        var request = new GetPreSignedUrlRequest() { BucketName = _config.Bucket, Key = path, Expires = expiresAt };

        var response = client.GetPreSignedURL(request);

        if (response.HasValue() && (_credentials?.Url?.StartsWith("http://") ?? false))
        {
            response = response.Replace("https://", "http://");
        }

        return response;
    }


    private string ProcessPath(string fileName)
    {
        if (_config.BasePath.HasValue())
        {
            return $"{_config.BasePath}/{fileName}";
        }

        return fileName;
    }
}