using Application.DataTransferObjects.Pagination;
using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Responses;
using Domain.Entities;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class MessageAttachmentRepository(ChatDb db) : IMessageAttachmentRepository
{
    public Task<bool> IsChatMemberAsync(int chatId, int userId, CancellationToken cancellationToken = default) => db.ChatMembers.AnyAsync(member => member.ChatId == chatId && member.UserId == userId, cancellationToken);
    public Task<bool> MessageExistsInChatAsync(int messageId, int chatId, CancellationToken cancellationToken = default) => db.Messages.AnyAsync(message => message.Id == messageId && message.ChatId == chatId, cancellationToken);
    public async Task AddAsync(Message message, Domain.Entities.File file, Photo photo, Attachment attachment, CancellationToken cancellationToken = default)
    {
        await db.Set<Domain.Entities.File>().AddAsync(file, cancellationToken);
        await db.Photos.AddAsync(photo, cancellationToken);
        await db.Messages.AddAsync(message, cancellationToken);
        await db.Attachments.AddAsync(attachment, cancellationToken);
    }
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
    public Task<Message?> GetMessageAsync(int messageId, CancellationToken cancellationToken = default) => db.Messages.AsNoTracking().Include(message => message.Sender).Include(message => message.Attachments).ThenInclude(attachment => attachment.File).FirstOrDefaultAsync(message => message.Id == messageId, cancellationToken);
    public Task<Photo?> GetPhotoForMemberAsync(int fileId, int userId, CancellationToken cancellationToken = default) => db.Photos.AsNoTracking().Include(photo => photo.File).Where(photo => photo.FileId == fileId && photo.File.Attachments.Any(attachment => attachment.Message.Chat.Members.Any(member => member.UserId == userId))).FirstOrDefaultAsync(cancellationToken);
    public async Task<IReadOnlyCollection<int>> GetMemberUserIdsAsync(int chatId, CancellationToken cancellationToken = default) => await db.ChatMembers.Where(member => member.ChatId == chatId).Select(member => member.UserId).ToArrayAsync(cancellationToken);

    public async Task<AttachmentSummaryDto> GetAttachmentSummaryAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var counts = await db.Attachments.AsNoTracking()
            .Where(attachment => attachment.Message.ChatId == chatId)
            .GroupBy(attachment => attachment.Type)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        int CountOf(AttachmentType type) => counts.FirstOrDefault(count => count.Key == type)?.Count ?? 0;
        return new(CountOf(AttachmentType.Photo), CountOf(AttachmentType.Video), CountOf(AttachmentType.File));
    }

    public Task<CursorPagedResponse<MessageAttachmentDto>> GetAttachmentsAsync(int chatId, AttachmentType type, CursorPaginationRequest pagination, CancellationToken cancellationToken = default) =>
        db.Attachments.AsNoTracking()
            .Where(attachment => attachment.Message.ChatId == chatId && attachment.Type == type)
            .ToCursorPagedResponseAsync(pagination, attachment => attachment.FileId, attachment => new MessageAttachmentDto(
                attachment.FileId, (int)attachment.Type, attachment.File.Name, attachment.File.MimeType, attachment.File.SizeBytes,
                attachment.Type == AttachmentType.Photo ? db.Photos.Where(photo => photo.FileId == attachment.FileId).Select(photo => photo.Width).FirstOrDefault() : 0,
                attachment.Type == AttachmentType.Photo ? db.Photos.Where(photo => photo.FileId == attachment.FileId).Select(photo => photo.Height).FirstOrDefault() : 0,
                $"/api/media/images/{attachment.FileId}"),
                dto => dto.Id, cancellationToken);
}
