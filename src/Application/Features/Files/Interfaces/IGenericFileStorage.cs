namespace Application.Features.Files;

public interface IGenericFileStorage
{
    Task<StoredGenericFileContent> SaveFileAsync(string fileName, string? declaredContentType, byte[] content, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}

public sealed record StoredGenericFileContent(string StoragePath, string MimeType);
