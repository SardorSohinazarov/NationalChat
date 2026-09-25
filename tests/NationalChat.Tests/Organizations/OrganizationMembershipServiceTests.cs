using Application.Features.Files;
using Application.Features.Groups;
using Application.Features.Groups.Validators;
using Application.Features.Messages;
using Application.Features.Organizations;
using Application.Features.Presence;
using Domain.Entities;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.Organizations;

public sealed class OrganizationMembershipServiceTests
{
    private readonly FakeGroupRepository _groups = new();
    private readonly FakeOrganizationRepository _organizations;
    private readonly OrganizationMembershipService _service;

    public OrganizationMembershipServiceTests()
    {
        _organizations = new FakeOrganizationRepository(_groups);
        var clock = new FixedTimeProvider(new DateTimeOffset(TestData.Start.AddDays(1)));
        var groupService = new GroupService(
            _groups,
            Substitute.For<IFileService>(),
            Substitute.For<IChatRealtimeNotifier>(),
            Substitute.For<IPresenceTracker>(),
            new CreateGroupRequestValidator(),
            new UpdateGroupRequestValidator(),
            new AddGroupMembersRequestValidator(),
            new UpdateGroupMemberRoleRequestValidator(),
            clock);
        _service = new OrganizationMembershipService(_organizations, groupService, clock);
    }

    private User AddUser(int id, string email)
    {
        var user = TestData.User(id, "User" + id);
        user.Email = email;
        _groups.Users.Add(user);
        return user;
    }

    private Group OrganizationGroup(Organization organization) =>
        Assert.Single(_groups.Groups, group => group.OrganizationId == organization.Id);

    [Fact]
    public async Task FirstUserFromDomain_CreatesOrganizationAndGroupAndIsAdmin()
    {
        var ali = AddUser(1, "ali@tuit.uz");

        await _service.EnsureMembershipAsync(ali);

        var organization = Assert.Single(_organizations.Organizations);
        Assert.Equal("tuit.uz", organization.Domain);
        Assert.Equal(OrganizationRole.Admin, Assert.Single(_organizations.Members).Role);

        var group = OrganizationGroup(organization);
        Assert.Equal("tuit.uz", group.Title);
        Assert.Null(group.InviteLink);
        var owner = Assert.Single(group.Chat.Members);
        Assert.Equal((ali.Id, ChatMemberRole.Creator), (owner.UserId, owner.Role));
        Assert.Equal(MessageServiceAction.GroupCreated, Assert.Single(group.Chat.Messages).ServiceAction);
    }

    [Fact]
    public async Task NextUserFromSameDomain_JoinsAsMemberWithServiceMessage()
    {
        var ali = AddUser(1, "ali@tuit.uz");
        var nodira = AddUser(2, "nodira@tuit.uz");
        await _service.EnsureMembershipAsync(ali);

        await _service.EnsureMembershipAsync(nodira);

        Assert.Single(_organizations.Organizations);
        Assert.Equal(OrganizationRole.Member, _organizations.Members.Single(m => m.UserId == nodira.Id).Role);
        var group = Assert.Single(_groups.Groups);
        Assert.Contains(group.Chat.Members, member => member.UserId == nodira.Id && member.Role == ChatMemberRole.Member);
        var joined = group.Chat.Messages.Last();
        Assert.Equal(MessageServiceAction.MemberJoinedViaOrganization, joined.ServiceAction);
        Assert.Equal("tuit.uz", joined.TextContent);
    }

    [Fact]
    public async Task Subdomain_IsSeparateOrganizationWithItsOwnGroup()
    {
        var ali = AddUser(1, "ali@tuit.uz");
        var vali = AddUser(2, "vali@student.tuit.uz");

        await _service.EnsureMembershipAsync(ali);
        await _service.EnsureMembershipAsync(vali);

        Assert.Equal(["tuit.uz", "student.tuit.uz"], _organizations.Organizations.Select(o => o.Domain));
        Assert.Equal(["tuit.uz", "student.tuit.uz"], _groups.Groups.Select(g => g.Title));
        Assert.All(_organizations.Members, member => Assert.Equal(OrganizationRole.Admin, member.Role));
        Assert.All(_groups.Groups, group => Assert.Single(group.Chat.Members));
    }

    [Fact]
    public async Task DifferentDomains_GetSeparateOrganizationsAndGroups()
    {
        await _service.EnsureMembershipAsync(AddUser(1, "ali@tuit.uz"));
        await _service.EnsureMembershipAsync(AddUser(2, "hr@rtm.uz"));

        Assert.Equal(["tuit.uz", "rtm.uz"], _organizations.Organizations.Select(o => o.Domain));
        Assert.Equal(["tuit.uz", "rtm.uz"], _groups.Groups.Select(g => g.Title));
        Assert.All(_organizations.Members, member => Assert.Equal(OrganizationRole.Admin, member.Role));
    }

    [Theory]
    [InlineData("ali@gmail.com")]
    [InlineData("ali@mail.ru")]
    [InlineData("ali@umail.uz")]
    public async Task PublicMailUser_GetsNothing(string email)
    {
        await _service.EnsureMembershipAsync(AddUser(1, email));

        Assert.Empty(_organizations.Organizations);
        Assert.Empty(_organizations.Members);
        Assert.Empty(_groups.Groups);
    }

    [Fact]
    public async Task RepeatedSignIn_DoesNotJoinAgain()
    {
        var ali = AddUser(1, "ali@tuit.uz");
        var vali = AddUser(2, "vali@tuit.uz");
        await _service.EnsureMembershipAsync(ali);
        await _service.EnsureMembershipAsync(vali);
        var group = Assert.Single(_groups.Groups);
        // Vali leaves; the next sign-in must not pull him back or create another group.
        group.Chat.Members.Remove(group.Chat.Members.Single(member => member.UserId == vali.Id));
        var messageCount = group.Chat.Messages.Count;

        await _service.EnsureMembershipAsync(vali);

        Assert.Equal(2, _organizations.Members.Count);
        Assert.DoesNotContain(group.Chat.Members, member => member.UserId == vali.Id);
        Assert.Equal(messageCount, group.Chat.Messages.Count);
        Assert.Single(_groups.Groups);
    }

    [Fact]
    public async Task DeletedGroup_IsRecreatedWithExistingMembers()
    {
        var ali = AddUser(1, "ali@tuit.uz");
        await _service.EnsureMembershipAsync(ali);
        _groups.Groups.Clear();
        var vali = AddUser(2, "vali@tuit.uz");

        await _service.EnsureMembershipAsync(vali);

        var group = Assert.Single(_groups.Groups);
        Assert.Equal(vali.Id, group.CreatorId);
        Assert.Equal([ali.Id, vali.Id], group.Chat.Members.Select(member => member.UserId).Order());
    }
}
