namespace Application.Features.Files;

public interface IFileStorage
{
    Task<StoredFileContent> SaveImageAsync(string fileName, byte[] content, bool cropThumbnail, CancellationToken cancellationToken = default);
    Task<StoredFileContent> SaveFileAsync(string fileName, string? declaredContentType, byte[] content, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}

public sealed record StoredFileContent(string StoragePath, string MimeType, int Width, int Height);
