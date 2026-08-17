namespace Application.Features.Files;

public static class ThumbnailPaths
{
    public static string ForOriginal(string storagePath, string? extensionOverride = null)
    {
        var directory = Path.GetDirectoryName(storagePath)?.Replace('\\', '/') ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(storagePath);
        var extension = extensionOverride ?? Path.GetExtension(storagePath);
        return string.IsNullOrEmpty(directory) ? $"{fileName}_thumb{extension}" : $"{directory}/{fileName}_thumb{extension}";
    }
}
