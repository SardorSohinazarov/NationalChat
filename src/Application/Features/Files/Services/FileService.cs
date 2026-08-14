using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Files.DataTransferObjects.Responses;

namespace Application.Features.Files;

public sealed class FileService(IFileStorage storage, IAntivirusScanner antivirusScanner) : IFileService
{
    private const int MaxImageSizeBytes = 10 * 1024 * 1024;
    private const int MaxFileSizeBytes = 50 * 1024 * 1024;

    public async Task<FileStoreResult> StoreImageAsync(StoreImageRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || request.Content.Length == 0) return new(null, "Rasm tanlanmagan.");
        if (request.Content.Length > MaxImageSizeBytes) return new(null, "Rasm hajmi 10 MB dan oshmasligi kerak.");
        try
        {
            if (!await antivirusScanner.IsCleanAsync(request.Content, cancellationToken)) return new(null, "Rasm xavfsizlik tekshiruvidan o'tmadi.");
            var stored = await storage.SaveImageAsync(request.FileName, request.Content, request.CropThumbnail, cancellationToken);
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
        if (request.Content.Length > MaxFileSizeBytes) return new(null, "Fayl hajmi 50 MB dan oshmasligi kerak.");
        try
        {
            if (!await antivirusScanner.IsCleanAsync(request.Content, cancellationToken)) return new(null, "Fayl xavfsizlik tekshiruvidan o'tmadi.");
            var stored = await storage.SaveFileAsync(request.FileName, request.DeclaredContentType, request.Content, cancellationToken);
            return new(new StoredFile(stored.StoragePath, Path.GetFileName(request.FileName), stored.MimeType, request.Content.Length, stored.Width, stored.Height), null);
        }
        catch (InvalidOperationException exception)
        {
            return new(null, exception.Message);
        }
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default) =>
        storage.OpenReadAsync(storagePath, cancellationToken);

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default) =>
        storage.DeleteAsync(storagePath, cancellationToken);
}
