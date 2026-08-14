using Application.Features.Files;

namespace Infrastructure.Storage;

public sealed class LocalFileStorage(string webRootPath) : IGenericFileStorage
{
    private readonly string rootPath = Path.Combine(webRootPath, "uploads", "files");

    public async Task<StoredGenericFileContent> SaveFileAsync(string fileName, string? declaredContentType, byte[] content, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var id = Guid.NewGuid().ToString("N");
        var dateDirectory = DateTime.UtcNow.ToString("yyyy/MM");
        var directory = Path.Combine(rootPath, dateDirectory);
        Directory.CreateDirectory(directory);

        var relativePath = Path.Combine("uploads", "files", dateDirectory, $"{id}{extension}").Replace('\\', '/');
        var fullPath = Path.Combine(webRootPath, relativePath);
        await System.IO.File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        var mimeType = string.IsNullOrWhiteSpace(declaredContentType) ? "application/octet-stream" : declaredContentType;
        return new StoredGenericFileContent(relativePath, mimeType);
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
}
