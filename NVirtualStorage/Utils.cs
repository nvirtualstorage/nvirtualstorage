namespace NVirtualStorage;

internal static class Utils
{
    internal static bool HasValue(this string? val)
    {
        return !String.IsNullOrWhiteSpace(val);
    }
}