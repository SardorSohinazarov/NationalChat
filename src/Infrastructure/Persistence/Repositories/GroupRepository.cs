using Application.Features.Groups;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class GroupRepository(ChatDb db) : IGroupRepository
{
    public Task<Group?> GetGroupAsync(int chatId, CancellationToken cancellationToken = default) =>
        db.Groups
            .Include(group => group.Chat)
                .ThenInclude(chat => chat.Members)
                .ThenInclude(member => member.User)
                .ThenInclude(user => user.Sessions)
            .Include(group => group.Photo)
                .ThenInclude(photo => photo!.File)
            .AsSplitQuery()
            .FirstOrDefaultAsync(group => group.ChatId == chatId && group.Chat.Type == ChatType.Group && group.Chat.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<User>> FindUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default) =>
        await db.Users
            .Include(user => user.Sessions)
            .Where(user => userIds.Contains(user.Id) && user.IsProfileCompleted)
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
