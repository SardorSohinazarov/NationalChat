using Domain.Entities;

namespace Application.Features.Groups;

public interface IGroupRepository
{
    /// <summary>Loads a tracked group with its chat, members (with users and sessions) and photo.</summary>
    Task<Group?> GetGroupAsync(int chatId, CancellationToken cancellationToken = default);

    /// <summary>Returns tracked users with completed profiles among <paramref name="userIds"/>.</summary>
    Task<IReadOnlyList<User>> FindUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default);

    Task AddGroupAsync(Group group, CancellationToken cancellationToken = default);
    Task AddPhotoAsync(Domain.Entities.File file, Photo photo, CancellationToken cancellationToken = default);
    void RemoveMember(ChatMember member);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
