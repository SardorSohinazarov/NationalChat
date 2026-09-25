using Application.Features.Groups;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class GroupRepository(ChatDb db) : IGroupRepository
{
    public Task<Group?> GetGroupAsync(int chatId, CancellationToken cancellationToken = default) =>
        LoadGroups().FirstOrDefaultAsync(group => group.ChatId == chatId, cancellationToken);

    public Task<Group?> GetGroupByInviteTokenAsync(string token, CancellationToken cancellationToken = default) =>
        LoadGroups().FirstOrDefaultAsync(group => group.InviteLink == token, cancellationToken);

    private IQueryable<Group> LoadGroups() =>
        db.Groups
            .Include(group => group.Chat)
                .ThenInclude(chat => chat.Members)
                .ThenInclude(member => member.User)
                .ThenInclude(user => user.Sessions)
            .Include(group => group.Chat)
                .ThenInclude(chat => chat.Members)
                .ThenInclude(member => member.User)
                .ThenInclude(user => user.OrganizationMembership!)
                .ThenInclude(membership => membership.Organization)
            .Include(group => group.Organization)
            .Include(group => group.Photo)
                .ThenInclude(photo => photo!.File)
            .AsSplitQuery()
            .Where(group => group.Chat.Type == ChatType.Group && group.Chat.DeletedAt == null);

    public async Task<IReadOnlyList<User>> FindUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default) =>
        await db.Users
            .Include(user => user.Sessions)
            .Include(user => user.OrganizationMembership!)
                .ThenInclude(membership => membership.Organization)
            .Where(user => userIds.Contains(user.Id) && user.IsProfileCompleted)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> FindOrganizationUsersAsync(int organizationId, IReadOnlyCollection<int> excludedUserIds, int limit, CancellationToken cancellationToken = default) =>
        limit <= 0
            ? []
            : await db.Users
                .Include(user => user.Sessions)
                .Include(user => user.OrganizationMembership!)
                    .ThenInclude(membership => membership.Organization)
                .Where(user => user.IsProfileCompleted && !excludedUserIds.Contains(user.Id) &&
                    user.OrganizationMembership != null && user.OrganizationMembership.OrganizationId == organizationId)
                .OrderBy(user => user.Id)
                .Take(limit)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

    public Task AddGroupAsync(Group group, CancellationToken cancellationToken = default) =>
        db.Groups.AddAsync(group, cancellationToken).AsTask();

    public async Task AddPhotoAsync(Domain.Entities.File file, Photo photo, CancellationToken cancellationToken = default)
    {
        await db.Set<Domain.Entities.File>().AddAsync(file, cancellationToken);
        await db.Photos.AddAsync(photo, cancellationToken);
    }

    public void RemoveMember(ChatMember member) => db.ChatMembers.Remove(member);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
