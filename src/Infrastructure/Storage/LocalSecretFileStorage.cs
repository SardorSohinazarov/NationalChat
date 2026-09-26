using Application.Features.SecretChats;

namespace Infrastructure.Storage;

/// <summary>
/// Encrypted secret chat attachments on local disk, outside the web root. File names are server-generated ids,
/// so no client input ever reaches a path.
/// </summary>
public sealed class LocalSecretFileStorage(string rootPath) : ISecretFileStorage
{
    public async Task<long?> SaveAsync(Guid fileId, Stream content, long maxBytes, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(rootPath);
        var path = PathOf(fileId);
        var buffer = new byte[81920];
        long total = 0;
        try
        {
            await using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, buffer.Length, useAsync: true))
            {
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    total += read;
                    if (total > maxBytes) break;
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
            }

            if (total <= maxBytes) return total;
            File.Delete(path);
            return null;
        }
        catch
        {
            File.Delete(path);
            throw;
        }
    }

    public Task<Stream?> OpenReadAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var path = PathOf(fileId);
        return Task.FromResult<Stream?>(File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true)
            : null);
    }

    public Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        File.Delete(PathOf(fileId));
        return Task.CompletedTask;
    }

    private string PathOf(Guid fileId) => Path.Combine(rootPath, fileId.ToString("N") + ".bin");
}
