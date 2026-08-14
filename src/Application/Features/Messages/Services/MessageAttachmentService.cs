using Application.Features.Files;
using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Messages.DataTransferObjects.Requests;
using Application.Features.Messages.DataTransferObjects.Responses;
using Application.Features.Messages.Mappers;
using Domain.Entities;

namespace Application.Features.Messages;

public sealed class MessageAttachmentService(
    IMessageAttachmentRepository repository,
    IFileService fileService,
    IChatRealtimeNotifier realtimeNotifier,
    TimeProvider timeProvider) : IMessageAttachmentService
{
    public async Task<MessageAttachmentResult> SendImageAsync(int currentUserId, int chatId, SendImageAttachmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.TextContent?.Length > 4_000) return new(null, "Xabar 4000 belgidan oshmasligi kerak.");
        if (!await repository.IsChatMemberAsync(chatId, currentUserId, cancellationToken)) return new(null, "Bu chatga rasm yuborish huquqi yo'q.");
        if (request.ReplyToMessageId.HasValue && !await repository.MessageExistsInChatAsync(request.ReplyToMessageId.Value, chatId, cancellationToken)) return new(null, "Javob yozilayotgan xabar topilmadi.");

        var storedResult = await fileService.StoreImageAsync(new StoreImageRequest(request.FileName, request.DeclaredContentType, request.Content), cancellationToken);
        if (storedResult.File is null) return new(null, storedResult.Error);
        var stored = storedResult.File;

        try
        {
            var file = new Domain.Entities.File { Name = stored.FileName, MimeType = stored.MimeType, SizeBytes = stored.SizeBytes, StoragePath = stored.StoragePath };
            var photo = new Photo { File = file, Width = stored.Width, Height = stored.Height };
            var message = new Message { ChatId = chatId, SenderId = currentUserId, TextContent = request.TextContent?.Trim() ?? string.Empty, ReplyToMessageId = request.ReplyToMessageId, SentAt = timeProvider.GetUtcNow().UtcDateTime };
            var attachment = new Attachment { Message = message, File = file, Type = AttachmentType.Photo };
            await repository.AddAsync(message, file, photo, attachment, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            var persisted = await repository.GetMessageAsync(message.Id, cancellationToken) ?? throw new InvalidOperationException("Yuborilgan rasm topilmadi.");
            var dto = MessageMapper.ToDto(persisted) with { Attachments = [new MessageAttachmentDto(file.Id, (int)AttachmentType.Photo, file.Name, file.MimeType, file.SizeBytes, photo.Width, photo.Height, $"/api/media/images/{file.Id}")] };
            await realtimeNotifier.MessageCreatedAsync(dto, await repository.GetMemberUserIdsAsync(chatId, cancellationToken), cancellationToken);
            return new(dto, null);
        }
        catch
        {
            await fileService.DeleteAsync(stored.StoragePath, cancellationToken);
            throw;
        }
    }

    public async Task<ProtectedAttachment?> GetImageAsync(int currentUserId, int fileId, CancellationToken cancellationToken = default)
    {
        var photo = await repository.GetPhotoForMemberAsync(fileId, currentUserId, cancellationToken);
        if (photo?.File is null) return null;
        var stream = await fileService.OpenReadAsync(photo.File.StoragePath, cancellationToken);
        return stream is null ? null : new(stream, photo.File.MimeType, photo.File.Name);
    }

    public async Task<MessageAttachmentResult> SendFileAsync(int currentUserId, int chatId, SendFileAttachmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.TextContent?.Length > 4_000) return new(null, "Xabar 4000 belgidan oshmasligi kerak.");
        if (!await repository.IsChatMemberAsync(chatId, currentUserId, cancellationToken)) return new(null, "Bu chatga fayl yuborish huquqi yo'q.");
        if (request.ReplyToMessageId.HasValue && !await repository.MessageExistsInChatAsync(request.ReplyToMessageId.Value, chatId, cancellationToken)) return new(null, "Javob yozilayotgan xabar topilmadi.");

        var storedResult = await fileService.StoreFileAsync(new StoreFileRequest(request.FileName, request.DeclaredContentType, request.Content), cancellationToken);
        if (storedResult.File is null) return new(null, storedResult.Error);
        var stored = storedResult.File;

        try
        {
            var file = new Domain.Entities.File { Name = stored.FileName, MimeType = stored.MimeType, SizeBytes = stored.SizeBytes, StoragePath = stored.StoragePath };
            var message = new Message { ChatId = chatId, SenderId = currentUserId, TextContent = request.TextContent?.Trim() ?? string.Empty, ReplyToMessageId = request.ReplyToMessageId, SentAt = timeProvider.GetUtcNow().UtcDateTime };
            var attachment = new Attachment { Message = message, File = file, Type = AttachmentType.File };
            await repository.AddFileMessageAsync(message, file, attachment, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            var persisted = await repository.GetMessageAsync(message.Id, cancellationToken) ?? throw new InvalidOperationException("Yuborilgan fayl topilmadi.");
            var dto = MessageMapper.ToDto(persisted) with { Attachments = [new MessageAttachmentDto(file.Id, (int)AttachmentType.File, file.Name, file.MimeType, file.SizeBytes, 0, 0, $"/api/media/files/{file.Id}")] };
            await realtimeNotifier.MessageCreatedAsync(dto, await repository.GetMemberUserIdsAsync(chatId, cancellationToken), cancellationToken);
            return new(dto, null);
        }
        catch
        {
            await fileService.DeleteFileAsync(stored.StoragePath, cancellationToken);
            throw;
        }
    }

    public async Task<ProtectedAttachment?> GetFileAsync(int currentUserId, int fileId, CancellationToken cancellationToken = default)
    {
        var file = await repository.GetFileForMemberAsync(fileId, currentUserId, cancellationToken);
        if (file is null) return null;
        var stream = await fileService.OpenFileReadAsync(file.StoragePath, cancellationToken);
        return stream is null ? null : new(stream, file.MimeType, file.Name);
    }
}
