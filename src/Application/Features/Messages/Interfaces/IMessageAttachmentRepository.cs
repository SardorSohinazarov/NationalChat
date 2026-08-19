using Application.DataTransferObjects.Pagination;
using Application.Features.Messages.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Messages;

public interface IMessageAttachmentRepository
{
    Task<bool> IsChatMemberAsync(int chatId, int userId, CancellationToken cancellationToken = default);
    Task<bool> MessageExistsInChatAsync(int messageId, int chatId, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, Domain.Entities.File file, Photo photo, Attachment attachment, CancellationToken cancellationToken = default);
    Task AddFileAsync(Message message, Domain.Entities.File file, Attachment attachment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Message?> GetMessageAsync(int messageId, CancellationToken cancellationToken = default);
    Task<Photo?> GetPhotoForMemberAsync(int fileId, int userId, CancellationToken cancellationToken = default);
    Task<Domain.Entities.File?> GetFileForMemberAsync(int fileId, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> GetMemberUserIdsAsync(int chatId, CancellationToken cancellationToken = default);
    Task<AttachmentSummaryDto> GetAttachmentSummaryAsync(int chatId, CancellationToken cancellationToken = default);
    Task<CursorPagedResponse<MessageAttachmentDto>> GetAttachmentsAsync(int chatId, AttachmentType type, CursorPaginationRequest pagination, CancellationToken cancellationToken = default);
}
