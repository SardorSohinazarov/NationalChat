using Application.Features.SecretChats;

namespace NationalChat.Tests.Support;

/// <summary>In-memory <see cref="ISecretFileStorage"/>.</summary>
public sealed class FakeSecretFileStorage : ISecretFileStorage
{
    public Dictionary<Guid, byte[]> Blobs { get; } = [];

    public async Task<long?> SaveAsync(Guid fileId, Stream content, long maxBytes, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length > maxBytes) return null;
        Blobs[fileId] = buffer.ToArray();
        return buffer.Length;
    }

    public Task<Stream?> OpenReadAsync(Guid fileId, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream?>(Blobs.TryGetValue(fileId, out var bytes) ? new MemoryStream(bytes, writable: false) : null);

    public Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        Blobs.Remove(fileId);
        return Task.CompletedTask;
    }
}
