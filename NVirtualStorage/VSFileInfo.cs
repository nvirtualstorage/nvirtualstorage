namespace NVirtualStorage;

public class VSFileInfo
{
    public string Name { get; set; }
    public string? InternalPath { get; set; }
    public string? ContentType { get; set; }

    public long? Size { get; set; }
    //public DateTimeOffset LastModified { get; set; }
    //public bool Exists { get; set; }
}