using Application.Features.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace Infrastructure.Storage;

public sealed class LocalImageStorage(string webRootPath) : IFileStorage
{
    private readonly string rootPath = Path.Combine(webRootPath, "uploads", "images");

    public async Task<StoredFileContent> SaveImageAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        await using var source = new MemoryStream(content, writable: false);
        IImageFormat? detectedFormat = await Image.DetectFormatAsync(source, cancellationToken);
        if (detectedFormat is null || !IsSupported(detectedFormat))
            throw new InvalidOperationException("Faqat JPEG, PNG va WebP rasmlar qabul qilinadi.");

        source.Position = 0;
        using var image = await Image.LoadAsync(source, cancellationToken);
        if (image.Width is <= 0 or > 10_000 || image.Height is <= 0 or > 10_000 || (long)image.Width * image.Height > 40_000_000)
            throw new InvalidOperationException("Rasm o'lchamlari ruxsat etilgan chegaradan tashqarida.");

        var extension = GetExtension(detectedFormat);
        var id = Guid.NewGuid().ToString("N");
        var dateDirectory = DateTime.UtcNow.ToString("yyyy/MM");
        var directory = Path.Combine(rootPath, dateDirectory);
        Directory.CreateDirectory(directory);

        var originalRelativePath = Path.Combine("uploads", "images", dateDirectory, $"{id}{extension}").Replace('\\', '/');
        var originalPath = Path.Combine(webRootPath, originalRelativePath);

        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;
        await image.SaveAsync(originalPath, GetEncoder(detectedFormat), cancellationToken);

        return new StoredFileContent(originalRelativePath, GetMimeType(detectedFormat), image.Width, image.Height);
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var safeRoot = Path.GetFullPath(rootPath);
        if (!fullPath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(System.IO.File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        if (fullPath.StartsWith(Path.GetFullPath(rootPath), StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
        return Task.CompletedTask;
    }

    private static bool IsSupported(IImageFormat format) => format is JpegFormat or PngFormat or WebpFormat;
    private static string GetExtension(IImageFormat format) => format is JpegFormat ? ".jpg" : format is PngFormat ? ".png" : ".webp";
    private static string GetMimeType(IImageFormat format) => format is JpegFormat ? "image/jpeg" : format is PngFormat ? "image/png" : "image/webp";
    private static IImageEncoder GetEncoder(IImageFormat format) => format switch
    {
        JpegFormat => new JpegEncoder { Quality = 90 },
        PngFormat => new PngEncoder(),
        WebpFormat => new WebpEncoder { Quality = 90 },
        _ => throw new InvalidOperationException("Rasm formati qo'llab-quvvatlanmaydi.")
    };
}
