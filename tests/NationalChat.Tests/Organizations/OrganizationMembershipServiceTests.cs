using Application.Features.Files;
using Application.Features.Groups;
using Application.Features.Groups.Validators;
using Application.Features.Messages;
using Application.Features.Organizations;
using Application.Features.Organizations.Options;
using Application.Features.Presence;
using Domain.Entities;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.Organizations;

public sealed class OrganizationMembershipServiceTests
{
    private readonly FakeGroupRepository _groups = new();
    private readonly FakeOrganizationRepository _organizations;
    private readonly IChatRealtimeNotifier _notifier = Substitute.For<IChatRealtimeNotifier>();
    private readonly OrganizationMembershipService _service;
    private readonly Organization _tatu = TestData.Organization(1, "TATU", "tuit.uz", "student.tuit.uz");

    public OrganizationMembershipServiceTests()
    {
        _organizations = new FakeOrganizationRepository(_groups);
        _organizations.Organizations.Add(_tatu);
        var clock = new FixedTimeProvider(new DateTimeOffset(TestData.Start.AddDays(1)));
        var groupService = new GroupService(
            _groups,
            Substitute.For<IFileService>(),
            _notifier,
            Substitute.For<IPresenceTracker>(),
            new CreateGroupRequestValidator(),
            new UpdateGroupRequestValidator(),
            new AddGroupMembersRequestValidator(),
            new UpdateGroupMemberRoleRequestValidator(),
            clock);
        var catalog = new OrganizationCatalog(
        [
            new OrganizationOptions { Name = "TATU", ShortName = "TATU", Domains = ["tuit.uz"], AdminEmails = ["Rektor@tuit.uz"] }
        ]);
        _service = new OrganizationMembershipService(_organizations, groupService, catalog, clock);
    }

    private User AddUser(int id, string email)
    {
        var user = TestData.User(id, "User" + id);
        user.Email = email;
        _groups.Users.Add(user);
        return user;
    }

    [Theory]
    [InlineData("ali@tuit.uz")]
    [InlineData("ALI@Student.Tuit.Uz")]
    public async Task OrganizationEmail_BecomesMember(string email)
    {
        var user = AddUser(1, email);

        await _service.EnsureMembershipAsync(user);

        var member = Assert.Single(_organizations.Members);
        Assert.Equal(_tatu.Id, member.OrganizationId);
        Assert.Equal(OrganizationRole.Member, member.Role);
    }

    [Fact]
    public async Task AdminEmail_BecomesAdmin()
    {
        var user = AddUser(1, "rektor@tuit.uz");

        await _service.EnsureMembershipAsync(user);

        Assert.Equal(OrganizationRole.Admin, Assert.Single(_organizations.Members).Role);
    }

    [Theory]
    [InlineData("ali@gmail.com")]
    [InlineData("ali@evil-tuit.uz")]
    [InlineData("ali@tuit.uz.evil.com")]
    public async Task OtherEmail_IsNotMember(string email)
    {
        await _service.EnsureMembershipAsync(AddUser(1, email));

        Assert.Empty(_organizations.Members);
    }

    [Fact]
    public async Task PublicDomain_IsIgnoredEvenIfRegistered()
    {
        // The sync rejects such configuration, but membership must not trust the database blindly either.
        _tatu.Domains.Add(new OrganizationDomain { Domain = "gmail.com", OrganizationId = _tatu.Id });

        await _service.EnsureMembershipAsync(AddUser(1, "ali@gmail.com"));

        Assert.Empty(_organizations.Members);
    }

    [Fact]
    public async Task NewMember_JoinsAutoJoinGroupsWithServiceMessage()
    {
        var admin = AddUser(1, "rektor@tuit.uz").Verified(_tatu, OrganizationRole.Admin);
        var autoJoin = TestData.Group(10, "TATU — umumiy", (admin, ChatMemberRole.Creator));
        autoJoin.OrganizationId = _tatu.Id;
        autoJoin.Organization = _tatu;
        autoJoin.AutoJoin = true;
        var manual = TestData.Group(20, "TATU — kafedra", (admin, ChatMemberRole.Creator));
        manual.OrganizationId = _tatu.Id;
        manual.Organization = _tatu;
        _groups.Groups.AddRange([autoJoin, manual]);
        var ali = AddUser(2, "ali@tuit.uz");

        await _service.EnsureMembershipAsync(ali);

        Assert.Contains(autoJoin.Chat.Members, member => member.UserId == ali.Id && member.Role == ChatMemberRole.Member);
        Assert.DoesNotContain(manual.Chat.Members, member => member.UserId == ali.Id);
        var message = Assert.Single(autoJoin.Chat.Messages);
        Assert.Equal(MessageServiceAction.MemberJoinedViaOrganization, message.ServiceAction);
        Assert.Equal(ali.Id, message.SenderId);
        Assert.Equal("TATU", message.TextContent);
    }

    [Fact]
    public async Task RepeatedSignIn_DoesNotJoinAgain()
    {
        var admin = AddUser(1, "rektor@tuit.uz").Verified(_tatu, OrganizationRole.Admin);
        var group = TestData.Group(10, "TATU — umumiy", (admin, ChatMemberRole.Creator));
        group.OrganizationId = _tatu.Id;
        group.Organization = _tatu;
        group.AutoJoin = true;
        _groups.Groups.Add(group);
        var ali = AddUser(2, "ali@tuit.uz");

        await _service.EnsureMembershipAsync(ali);
        // Ali leaves; the next sign-in must not pull him back.
        group.Chat.Members.Remove(group.Chat.Members.Single(member => member.UserId == ali.Id));
        await _service.EnsureMembershipAsync(ali);

        Assert.Single(_organizations.Members);
        Assert.DoesNotContain(group.Chat.Members, member => member.UserId == ali.Id);
        Assert.Single(group.Chat.Messages);
    }

    [Fact]
    public async Task RemovedDomain_DropsMembership()
    {
        var ali = AddUser(1, "ali@tuit.uz");
        await _service.EnsureMembershipAsync(ali);
        _tatu.Domains.Clear();

        await _service.EnsureMembershipAsync(ali);

        Assert.Empty(_organizations.Members);
        Assert.Null(ali.OrganizationMembership);
    }
}
