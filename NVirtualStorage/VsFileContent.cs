namespace NVirtualStorage;

public class VsFileContent : IDisposable
{
    public string Name { get; internal set; }
    public string? ContentType { get; set; }

    public long? Size { get; set; }

    public Stream? Contents { get; set; }
    public DateTime? LastUpdate { get; set; }

    public byte[] AsBytes()
    {
        if (Contents == null) throw new Exception("No contents");
        using var ms = new MemoryStream();
        Contents.CopyTo(ms);
        Contents.Flush();
        ms.Flush();
        return ms.ToArray();
    }

    public async Task<byte[]> AsBytesAsync()
    {
        if (Contents == null) throw new Exception("No contents");
        using var ms = new MemoryStream();
        await Contents.CopyToAsync(ms);
        Contents.Flush();
        ms.Flush();
        return ms.ToArray();
    }

    public void Dispose()
    {
        Contents?.Dispose();
    }
}