using Application.Features.Files;
using Infrastructure.Video;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Storage;

public sealed class LocalImageStorage(string webRootPath) : IFileStorage
{
    private const int ThumbnailSize = 256;
    private static readonly HashSet<string> BlockedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".sh", ".msi", ".dll", ".scr", ".com", ".ps1", ".vbs", ".vbe", ".js", ".jse", ".jar", ".app", ".apk", ".msix", ".cpl", ".gadget", ".lnk", ".reg"
    };
    private readonly string rootPath = Path.Combine(webRootPath, "uploads", "images");
    private readonly string filesRootPath = Path.Combine(webRootPath, "uploads", "files");
    private readonly string uploadsRootPath = Path.Combine(webRootPath, "uploads");

    public async Task<StoredFileContent> SaveImageAsync(string fileName, byte[] content, bool cropThumbnail, CancellationToken cancellationToken = default)
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

        if (image.Width > ThumbnailSize || image.Height > ThumbnailSize)
        {
            var thumbnailPath = Path.Combine(webRootPath, ThumbnailPaths.ForOriginal(originalRelativePath));
            using var thumbnail = image.Clone(context => context.Resize(new ResizeOptions
            {
                Size = new Size(ThumbnailSize, ThumbnailSize),
                Mode = cropThumbnail ? ResizeMode.Crop : ResizeMode.Max,
            }));
            await thumbnail.SaveAsync(thumbnailPath, GetEncoder(detectedFormat), cancellationToken);
        }

        return new StoredFileContent(originalRelativePath, GetMimeType(detectedFormat), image.Width, image.Height);
    }

    public async Task<StoredFileContent> SaveFileAsync(string fileName, string? declaredContentType, byte[] content, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || BlockedFileExtensions.Contains(extension))
            throw new InvalidOperationException("Ushbu turdagi fayl yuborish taqiqlangan.");

        var id = Guid.NewGuid().ToString("N");
        var dateDirectory = DateTime.UtcNow.ToString("yyyy/MM");
        var directory = Path.Combine(filesRootPath, dateDirectory);
        Directory.CreateDirectory(directory);

        var relativePath = Path.Combine("uploads", "files", dateDirectory, $"{id}{extension}").Replace('\\', '/');
        var fullPath = Path.Combine(webRootPath, relativePath);
        await System.IO.File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        var mimeType = string.IsNullOrWhiteSpace(declaredContentType) ? "application/octet-stream" : declaredContentType;
        if (!mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return new StoredFileContent(relativePath, mimeType, 0, 0);

        var probe = await FfmpegVideoProbe.ProbeAsync(fullPath, cancellationToken);
        if (probe is null) return new StoredFileContent(relativePath, mimeType, 0, 0);

        var thumbnailFullPath = Path.Combine(webRootPath, ThumbnailPaths.ForOriginal(relativePath, ".jpg"));
        await FfmpegVideoProbe.ExtractPosterFrameAsync(fullPath, thumbnailFullPath, probe.DurationSeconds, cancellationToken);
        return new StoredFileContent(relativePath, mimeType, probe.Width, probe.Height);
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var safeRoot = Path.GetFullPath(uploadsRootPath);
        if (!fullPath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(System.IO.File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        DeleteIfExists(relativePath);
        DeleteIfExists(ThumbnailPaths.ForOriginal(relativePath));
        DeleteIfExists(ThumbnailPaths.ForOriginal(relativePath, ".jpg"));
        return Task.CompletedTask;
    }

    private void DeleteIfExists(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        if (fullPath.StartsWith(Path.GetFullPath(uploadsRootPath), StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
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
