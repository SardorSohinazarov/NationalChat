using Domain.Entities;

namespace Application.Features.Messages;

public interface IMessageAttachmentRepository
{
    Task<bool> IsChatMemberAsync(int chatId, int userId, CancellationToken cancellationToken = default);
    Task<bool> MessageExistsInChatAsync(int messageId, int chatId, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, Domain.Entities.File file, Photo photo, Attachment attachment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Message?> GetMessageAsync(int messageId, CancellationToken cancellationToken = default);
    Task<Photo?> GetPhotoForMemberAsync(int fileId, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> GetMemberUserIdsAsync(int chatId, CancellationToken cancellationToken = default);
}
