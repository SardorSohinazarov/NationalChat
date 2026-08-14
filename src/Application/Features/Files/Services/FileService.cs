using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Files.DataTransferObjects.Responses;

namespace Application.Features.Files;

public sealed class FileService(IFileStorage storage, IGenericFileStorage fileStorage, IAntivirusScanner antivirusScanner) : IFileService
{
    private const int MaxImageSizeBytes = 10 * 1024 * 1024;
    private const int MaxFileSizeBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".sh", ".msi", ".dll", ".js", ".vbs", ".ps1", ".com", ".scr", ".jar", ".app", ".apk"
    };

    public async Task<FileStoreResult> StoreImageAsync(StoreImageRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || request.Content.Length == 0) return new(null, "Rasm tanlanmagan.");
        if (request.Content.Length > MaxImageSizeBytes) return new(null, "Rasm hajmi 10 MB dan oshmasligi kerak.");
        try
        {
            if (!await antivirusScanner.IsCleanAsync(request.Content, cancellationToken)) return new(null, "Rasm xavfsizlik tekshiruvidan o'tmadi.");
            var stored = await storage.SaveImageAsync(request.FileName, request.Content, cancellationToken);
            return new(new StoredFile(stored.StoragePath, Path.GetFileName(request.FileName), stored.MimeType, request.Content.Length, stored.Width, stored.Height), null);
        }
        catch (InvalidOperationException exception)
        {
            return new(null, exception.Message);
        }
    }

    public async Task<FileStoreResult> StoreFileAsync(StoreFileRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || request.Content.Length == 0) return new(null, "Fayl tanlanmagan.");
        if (request.Content.Length > MaxFileSizeBytes) return new(null, "Fayl hajmi 20 MB dan oshmasligi kerak.");

        var safeFileName = SanitizeFileName(request.FileName);
        var extension = Path.GetExtension(safeFileName);
        if (!string.IsNullOrEmpty(extension) && BlockedExtensions.Contains(extension)) return new(null, "Bu turdagi fayl qabul qilinmaydi.");

        if (!await antivirusScanner.IsCleanAsync(request.Content, cancellationToken)) return new(null, "Fayl xavfsizlik tekshiruvidan o'tmadi.");
        var stored = await fileStorage.SaveFileAsync(safeFileName, request.DeclaredContentType, request.Content, cancellationToken);
        return new(new StoredFile(stored.StoragePath, safeFileName, stored.MimeType, request.Content.Length, 0, 0), null);
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default) =>
        storage.OpenReadAsync(storagePath, cancellationToken);

    public Task<Stream?> OpenFileReadAsync(string storagePath, CancellationToken cancellationToken = default) =>
        fileStorage.OpenReadAsync(storagePath, cancellationToken);

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default) =>
        storage.DeleteAsync(storagePath, cancellationToken);

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default) =>
        fileStorage.DeleteAsync(storagePath, cancellationToken);

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(c => c != '\r' && c != '\n' && !invalidChars.Contains(c)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "fayl" : cleaned;
    }
}
