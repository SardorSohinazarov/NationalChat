namespace Application.Features.SecretChats;

/// <summary>Opaque storage for encrypted attachments, addressed only by server-generated ids.</summary>
public interface ISecretFileStorage
{
    /// <summary>Writes the stream; returns the size, or null (and keeps nothing) when it is longer than <paramref name="maxBytes"/>.</summary>
    Task<long?> SaveAsync(Guid fileId, Stream content, long maxBytes, CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>Removes the blob; a missing one is not an error.</summary>
    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}
