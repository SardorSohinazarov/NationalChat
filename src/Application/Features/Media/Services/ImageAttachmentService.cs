using Application.Features.Media.DataTransferObjects.Requests;
using Application.Features.Media.DataTransferObjects.Responses;
using Application.Features.Messages;
using Application.Features.Messages.Mappers;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Media;

public sealed class ImageAttachmentService(
    IImageAttachmentRepository repository,
    IImageStorage storage,
    IAntivirusScanner antivirusScanner,
    IChatRealtimeNotifier realtimeNotifier,
    IValidator<UploadImageRequest> validator,
    TimeProvider timeProvider) : IImageAttachmentService
{
    public async Task<ImageUploadResult> UploadAsync(int currentUserId, int chatId, UploadImageRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return new(null, validation.Errors[0].ErrorMessage);
        if (!await repository.IsChatMemberAsync(chatId, currentUserId, cancellationToken)) return new(null, "Bu chatga rasm yuborish huquqi yo'q.");
        if (request.ReplyToMessageId.HasValue && !await repository.MessageExistsInChatAsync(request.ReplyToMessageId.Value, chatId, cancellationToken))
            return new(null, "Javob yozilayotgan xabar topilmadi.");

        try
        {
            if (!await antivirusScanner.IsCleanAsync(request.Content, cancellationToken)) return new(null, "Rasm xavfsizlik tekshiruvidan o'tmadi.");
        }
        catch (InvalidOperationException exception)
        {
            return new(null, exception.Message);
        }

        StoredImage stored;
        try
        {
            stored = await storage.SaveAsync(request.FileName, request.DeclaredContentType ?? string.Empty, request.Content, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return new(null, exception.Message);
        }

        try
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var file = new Domain.Entities.File { Name = Path.GetFileName(request.FileName), MimeType = stored.MimeType, SizeBytes = request.Content.Length, StoragePath = stored.OriginalPath };
            var photo = new Photo { File = file, Width = stored.Width, Height = stored.Height };
            var message = new Message { ChatId = chatId, SenderId = currentUserId, TextContent = request.TextContent?.Trim() ?? string.Empty, ReplyToMessageId = request.ReplyToMessageId, SentAt = now };
            var attachment = new Attachment { Message = message, File = file, Type = AttachmentType.Photo };
            await repository.AddAsync(message, file, photo, attachment, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            var persisted = await repository.GetMessageAsync(message.Id, cancellationToken);
            if (persisted is null) throw new InvalidOperationException("Yuborilgan rasm topilmadi.");
            var dto = MessageMapper.ToDto(persisted) with
            {
                Attachments = [new MessageAttachmentDto(file.Id, (int)AttachmentType.Photo, file.Name, file.MimeType, file.SizeBytes, photo.Width, photo.Height, $"/api/media/images/{file.Id}")]
            };
            await realtimeNotifier.MessageCreatedAsync(dto, await repository.GetMemberUserIdsAsync(chatId, cancellationToken), cancellationToken);
            return new(dto, null);
        }
        catch
        {
            await storage.DeleteAsync(stored.OriginalPath, cancellationToken);
            throw;
        }
    }

    public async Task<ProtectedImage?> GetImageAsync(int currentUserId, int fileId, CancellationToken cancellationToken = default)
    {
        var photo = await repository.GetPhotoForMemberAsync(fileId, currentUserId, cancellationToken);
        if (photo?.File is null) return null;
        var stream = await storage.OpenReadAsync(photo.File.StoragePath, cancellationToken);
        return stream is null ? null : new(stream, photo.File.MimeType, photo.File.Name);
    }
}
