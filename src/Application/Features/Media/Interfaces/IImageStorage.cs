namespace Application.Features.Media;

public interface IImageStorage
{
    Task<StoredImage> SaveAsync(string fileName, string mimeType, byte[] content, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}

public sealed record StoredImage(string OriginalPath, string MimeType, int Width, int Height);
