using Domain.Entities;

namespace Application.Features.Chats;

public interface IChatRepository
{
    Task<User?> FindUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Chat?> FindPrivateChatAsync(int firstUserId, int secondUserId, CancellationToken cancellationToken = default);
    Task AddAsync(Chat chat, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
