using Domain.Entities;

namespace Application.Features.Groups;

public interface IGroupRepository
{
    /// <summary>Loads a tracked group with its chat, members (with users and sessions) and photo.</summary>
    Task<Group?> GetGroupAsync(int chatId, CancellationToken cancellationToken = default);

    /// <summary>Returns tracked users with completed profiles among <paramref name="userIds"/>, with their organization membership.</summary>
    Task<IReadOnlyList<User>> FindUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default);

    /// <summary>Tracked users with completed profiles who are verified members of the organization, except <paramref name="excludedUserIds"/>.</summary>
    Task<IReadOnlyList<User>> FindOrganizationUsersAsync(int organizationId, IReadOnlyCollection<int> excludedUserIds, int limit, CancellationToken cancellationToken = default);

    /// <summary>Same as <see cref="GetGroupAsync"/>, found by its invite link token.</summary>
    Task<Group?> GetGroupByInviteTokenAsync(string token, CancellationToken cancellationToken = default);

    Task AddGroupAsync(Group group, CancellationToken cancellationToken = default);
    Task AddPhotoAsync(Domain.Entities.File file, Photo photo, CancellationToken cancellationToken = default);
    void RemoveMember(ChatMember member);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
