namespace Application.Features.SecretChats;

/// <summary>Encrypted attachments of secret chats. As with messages, only the two bound devices may use them.</summary>
public interface ISecretFileService
{
    Task<SecretFileUploadResult> UploadAsync(int userId, int sessionId, int secretChatId, Stream content, long? contentLength, CancellationToken cancellationToken = default);

    /// <summary>The ciphertext, or null when the file or chat is not accessible from this device.</summary>
    Task<Stream?> OpenAsync(int userId, int sessionId, int secretChatId, Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>The recipient has the file (or the sender gives up on it): the server copy is deleted.</summary>
    Task<bool> DeleteAsync(int userId, int sessionId, int secretChatId, Guid fileId, CancellationToken cancellationToken = default);
}

public sealed record SecretFileUploadResult(Guid? FileId, string? Error);
