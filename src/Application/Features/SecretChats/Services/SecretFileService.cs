using Application.Features.SecretChats.Factories;
using Domain.Entities;

namespace Application.Features.SecretChats;

public sealed class SecretFileService(
    ISecretChatRepository repository,
    ISecretFileStorage storage,
    TimeProvider timeProvider) : ISecretFileService
{
    private const string NotFoundError = "Maxfiy chat topilmadi.";
    private const string TooLargeError = "Fayl 20 MB dan katta bo'lmasligi kerak.";

    public async Task<SecretFileUploadResult> UploadAsync(int userId, int sessionId, int secretChatId, Stream content, long? contentLength, CancellationToken cancellationToken = default)
    {
        if (contentLength is null or <= 0) return Fail("Fayl bo'sh.");
        if (contentLength > SecretChatLimits.MaxFileBytes) return Fail(TooLargeError);

        var chat = await BoundChatAsync(userId, sessionId, secretChatId, cancellationToken);
        if (chat is null) return Fail(NotFoundError);
        if (chat.Status != SecretChatStatus.Active) return Fail("Maxfiy chat faol emas.");
        if (await repository.CountFilesAsync(chat.Id, cancellationToken) >= SecretChatLimits.MaxUndeliveredFiles)
        {
            return Fail("Suhbatdosh hali oldingi fayllarni olmadi. Birozdan keyin urinib ko'ring.");
        }

        var fileId = Guid.NewGuid();
        // The declared length is only a hint: the stream is cut off at the limit whatever it claims.
        var size = await storage.SaveAsync(fileId, content, SecretChatLimits.MaxFileBytes, cancellationToken);
        if (size is null or 0)
        {
            await storage.DeleteAsync(fileId, CancellationToken.None);
            return Fail(size is null ? TooLargeError : "Fayl bo'sh.");
        }

        try
        {
            await repository.AddFileAsync(SecretChatFactory.CreateFile(fileId, chat.Id, sessionId, size.Value, timeProvider.GetUtcNow().UtcDateTime), cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(fileId, CancellationToken.None);
            throw;
        }

        return new(fileId, null);
    }

    public async Task<Stream?> OpenAsync(int userId, int sessionId, int secretChatId, Guid fileId, CancellationToken cancellationToken = default)
    {
        var chat = await BoundChatAsync(userId, sessionId, secretChatId, cancellationToken);
        if (chat is null || await repository.GetFileAsync(chat.Id, fileId, cancellationToken) is null) return null;
        return await storage.OpenReadAsync(fileId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int userId, int sessionId, int secretChatId, Guid fileId, CancellationToken cancellationToken = default)
    {
        var chat = await BoundChatAsync(userId, sessionId, secretChatId, cancellationToken);
        var file = chat is null ? null : await repository.GetFileAsync(chat.Id, fileId, cancellationToken);
        if (file is null) return false;

        repository.RemoveFile(file);
        await repository.SaveChangesAsync(cancellationToken);
        await storage.DeleteAsync(fileId, cancellationToken);
        return true;
    }

    private async Task<SecretChat?> BoundChatAsync(int userId, int sessionId, int secretChatId, CancellationToken cancellationToken)
    {
        if (!await repository.IsSessionActiveAsync(userId, sessionId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken)) return null;
        var chat = await repository.GetAsync(secretChatId, cancellationToken);
        return chat is not null && SecretChatAccess.IsBound(chat, userId, sessionId) ? chat : null;
    }

    private static SecretFileUploadResult Fail(string error) => new(null, error);
}
