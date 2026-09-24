using Application.Features.Groups;
using Application.Features.Organizations;
using Application.Features.Organizations.DataTransferObjects.Responses;
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

    public Task<IReadOnlyList<User>> FindOrganizationUsersAsync(int organizationId, IReadOnlyCollection<int> excludedUserIds, int limit, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<User>>(Users
            .Where(u => u.IsProfileCompleted && !excludedUserIds.Contains(u.Id) && u.OrganizationMembership?.OrganizationId == organizationId)
            .OrderBy(u => u.Id)
            .Take(limit)
            .ToList());

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

/// <summary>
/// In-memory <see cref="IOrganizationRepository"/>; memberships live on the users of the shared
/// <see cref="FakeGroupRepository"/> so group rules and membership rules see the same data.
/// </summary>
public sealed class FakeOrganizationRepository(FakeGroupRepository groups) : IOrganizationRepository
{
    private int _nextId = 5000;

    public List<Organization> Organizations { get; } = [];
    public List<OrganizationMember> Members { get; } = [];

    public Task<IReadOnlyList<Organization>> GetAllWithDomainsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Organization>>(Organizations.ToList());

    public Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        Organizations.Add(organization);
        return Task.CompletedTask;
    }

    public void RemoveDomain(OrganizationDomain domain)
    {
    }

    public Task RemoveMembersAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        foreach (var member in Members.Where(m => m.OrganizationId == organizationId).ToList()) RemoveMember(member);
        return Task.CompletedTask;
    }

    public Task<Organization?> FindActiveByDomainAsync(string domain, CancellationToken cancellationToken = default) =>
        Task.FromResult(Organizations.FirstOrDefault(o => o.IsActive && o.Domains.Any(d => d.Domain == domain)));

    public Task<OrganizationMember?> GetMembershipAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Members.FirstOrDefault(m => m.UserId == userId));

    public Task<bool> TryAddMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default)
    {
        if (Members.Any(m => m.UserId == member.UserId)) return Task.FromResult(false);
        member.Id = _nextId++;
        member.Organization = Organizations.Single(o => o.Id == member.OrganizationId);
        Members.Add(member);
        var user = groups.Users.FirstOrDefault(u => u.Id == member.UserId);
        if (user is not null)
        {
            member.User = user;
            user.OrganizationMembership = member;
        }

        return Task.FromResult(true);
    }

    public void RemoveMember(OrganizationMember member)
    {
        Members.Remove(member);
        var user = groups.Users.FirstOrDefault(u => u.Id == member.UserId);
        if (user?.OrganizationMembership == member) user.OrganizationMembership = null;
    }

    public Task<IReadOnlyList<int>> GetAutoJoinGroupChatIdsAsync(int organizationId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<int>>(groups.Groups.Where(g => g.OrganizationId == organizationId && g.AutoJoin).Select(g => g.ChatId).ToList());

    public Task<MyOrganizationDto?> GetMyOrganizationAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Members.Where(m => m.UserId == userId)
            .Select(m => (MyOrganizationDto?)new MyOrganizationDto(m.Organization.Id, m.Organization.Name, m.Organization.ShortName, m.Role))
            .FirstOrDefault());

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var organization in Organizations.Where(o => o.Id == 0)) organization.Id = _nextId++;
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

    public static Organization Organization(int id, string shortName, params string[] domains) => new()
    {
        Id = id,
        Name = shortName + " universiteti",
        ShortName = shortName,
        IsActive = true,
        CreatedAt = Start,
        Domains = domains.Select((domain, index) => new OrganizationDomain { Id = id * 10 + index, OrganizationId = id, Domain = domain }).ToList(),
    };

    /// <summary>Makes <paramref name="user"/> a verified member of <paramref name="organization"/>.</summary>
    public static User Verified(this User user, Organization organization, OrganizationRole role = OrganizationRole.Member)
    {
        user.OrganizationMembership = new OrganizationMember
        {
            Id = user.Id * 7,
            OrganizationId = organization.Id,
            Organization = organization,
            UserId = user.Id,
            User = user,
            Role = role,
            VerifiedAt = Start,
        };
        return user;
    }

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
