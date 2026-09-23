using Application.Features.Groups;
using Domain.Entities;

namespace NationalChat.Tests.Support;

/// <summary>Deterministic clock for time-dependent services.</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

/// <summary>
/// In-memory <see cref="IGroupRepository"/> that mimics what EF Core does on SaveChanges:
/// assigns identifiers to new chats, groups, members and messages.
/// </summary>
public sealed class FakeGroupRepository : IGroupRepository
{
    private int _nextId = 1000;

    public List<Group> Groups { get; } = [];
    public List<User> Users { get; } = [];
    public int SaveCount { get; private set; }

    public Task<Group?> GetGroupAsync(int chatId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Groups.FirstOrDefault(g => g.ChatId == chatId && g.Chat.DeletedAt == null));

    public Task<IReadOnlyList<User>> FindUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<User>>(Users.Where(u => userIds.Contains(u.Id) && u.IsProfileCompleted).ToList());

    public Task AddGroupAsync(Group group, CancellationToken cancellationToken = default)
    {
        Groups.Add(group);
        return Task.CompletedTask;
    }

    public Task AddPhotoAsync(Domain.Entities.File file, Photo photo, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void RemoveMember(ChatMember member)
    {
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        foreach (var group in Groups)
        {
            if (group.Chat.Id == 0) group.Chat.Id = _nextId++;
            if (group.Id == 0) group.Id = _nextId++;
            group.ChatId = group.Chat.Id;
            foreach (var member in group.Chat.Members)
            {
                if (member.Id == 0) member.Id = _nextId++;
                member.ChatId = group.ChatId;
            }
            foreach (var message in group.Chat.Messages)
            {
                if (message.Id == 0) message.Id = _nextId++;
                message.ChatId = group.ChatId;
            }
        }
        return Task.CompletedTask;
    }
}

/// <summary>Builds users and groups for tests.</summary>
public static class TestData
{
    public static readonly DateTime Start = new(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);

    public static User User(int id, string firstName, bool completed = true) => new()
    {
        Id = id,
        Email = $"{firstName.ToLowerInvariant()}@example.com",
        Username = $"{firstName.ToLowerInvariant()}_{id}",
        FirstName = firstName,
        IsProfileCompleted = completed,
        CreatedAt = Start,
    };

    /// <summary>
    /// Creates a group; members are added in the given order, one minute apart,
    /// so JoinedAt reflects seniority.
    /// </summary>
    public static Group Group(int chatId, string title, params (User User, ChatMemberRole Role)[] members)
    {
        var chat = new Chat { Id = chatId, Type = ChatType.Group, CreatedAt = Start };
        var memberId = chatId * 100;
        foreach (var (user, role) in members)
        {
            chat.Members.Add(new ChatMember
            {
                Id = ++memberId,
                ChatId = chatId,
                UserId = user.Id,
                User = user,
                Role = role,
                JoinedAt = Start.AddMinutes(memberId % 100),
            });
        }

        var creator = members.First(m => m.Role == ChatMemberRole.Creator).User;
        var group = new Group { Id = chatId, ChatId = chatId, Chat = chat, Title = title, CreatorId = creator.Id, Creator = creator };
        chat.Groups.Add(group);
        return group;
    }
}
