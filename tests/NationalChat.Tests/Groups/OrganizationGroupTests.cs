using Application.Features.Files;
using Application.Features.Groups;
using Application.Features.Groups.DataTransferObjects.Requests;
using Application.Features.Groups.Validators;
using Application.Features.Messages;
using Application.Features.Presence;
using Domain.Entities;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.Groups;

/// <summary>The private "tuit.uz" group: domain members join automatically, others only via an admin or the invite link.</summary>
public sealed class OrganizationGroupTests
{
    private readonly FakeGroupRepository _repository = new();
    private readonly GroupService _service;
    private readonly Organization _tuit = TestData.Organization(1, "tuit.uz");
    private readonly User _admin;
    private readonly User _member;
    private readonly User _colleague;
    private readonly User _outsider = TestData.User(4, "Outsider");

    public OrganizationGroupTests()
    {
        _admin = TestData.User(1, "Ali").Verified(_tuit, OrganizationRole.Admin);
        _member = TestData.User(2, "Nodira").Verified(_tuit);
        _colleague = TestData.User(3, "Vali").Verified(_tuit);
        _repository.Users.AddRange([_admin, _member, _colleague, _outsider]);
        _service = new GroupService(
            _repository,
            Substitute.For<IFileService>(),
            Substitute.For<IChatRealtimeNotifier>(),
            Substitute.For<IPresenceTracker>(),
            new CreateGroupRequestValidator(),
            new UpdateGroupRequestValidator(),
            new AddGroupMembersRequestValidator(),
            new UpdateGroupMemberRoleRequestValidator(),
            new FixedTimeProvider(new DateTimeOffset(TestData.Start.AddDays(1))));
    }

    /// <summary>Ali — ega, Nodira — a'zo.</summary>
    private Group OrganizationGroup()
    {
        var group = TestData.Group(20, "tuit.uz", (_admin, ChatMemberRole.Creator), (_member, ChatMemberRole.Member));
        group.OrganizationId = _tuit.Id;
        group.Organization = _tuit;
        _repository.Groups.Add(group);
        return group;
    }

    [Fact]
    public async Task CreateOrganizationGroup_IsNamedAfterDomainWithBadgesAndNoLink()
    {
        Assert.True(await _service.CreateOrganizationGroupAsync(_admin.Id));

        var group = Assert.Single(_repository.Groups);
        Assert.Equal("tuit.uz", group.Title);
        Assert.Equal(_tuit.Id, group.OrganizationId);
        Assert.Null(group.InviteLink);
        Assert.Equal([_admin.Id, _member.Id, _colleague.Id], group.Chat.Members.Select(m => m.UserId).Order());
        Assert.Equal(ChatMemberRole.Creator, group.Chat.Members.Single(m => m.UserId == _admin.Id).Role);

        var dto = await _service.GetAsync(_admin.Id, group.ChatId);
        Assert.Equal("tuit.uz", dto!.Organization!.Domain);
        Assert.All(dto.Members, member => Assert.Equal("tuit.uz", member.Organization!.Domain));
    }

    [Fact]
    public async Task CreateOrganizationGroup_WithoutOrganization_DoesNothing()
    {
        Assert.False(await _service.CreateOrganizationGroupAsync(_outsider.Id));
        Assert.Empty(_repository.Groups);
    }

    [Fact]
    public async Task Admin_CanAddOutsider()
    {
        var group = OrganizationGroup();

        var result = await _service.AddMembersAsync(_admin.Id, group.ChatId, new AddGroupMembersRequest([_outsider.Id]));

        Assert.Null(result.Error);
        Assert.Contains(group.Chat.Members, member => member.UserId == _outsider.Id);
    }

    [Fact]
    public async Task RegularMember_CannotAddAnyone()
    {
        var group = OrganizationGroup();

        var result = await _service.AddMembersAsync(_member.Id, group.ChatId, new AddGroupMembersRequest([_outsider.Id]));

        Assert.Null(result.Group);
        Assert.DoesNotContain(group.Chat.Members, member => member.UserId == _outsider.Id);
    }

    [Fact]
    public async Task InviteLink_OnlyAdminsCreateAndSeeIt()
    {
        var group = OrganizationGroup();

        Assert.Null((await _service.CreateInviteLinkAsync(_member.Id, group.ChatId)).Group);
        var created = await _service.CreateInviteLinkAsync(_admin.Id, group.ChatId);

        Assert.True(GroupInviteTokens.IsWellFormed(created.Group!.InviteToken));
        Assert.Equal(group.InviteLink, created.Group.InviteToken);
        Assert.Null((await _service.GetAsync(_member.Id, group.ChatId))!.InviteToken);
    }

    [Fact]
    public async Task InviteLink_LetsOutsiderJoinWithServiceMessage()
    {
        var group = OrganizationGroup();
        var token = (await _service.CreateInviteLinkAsync(_admin.Id, group.ChatId)).Group!.InviteToken!;

        var preview = await _service.GetInvitePreviewAsync(_outsider.Id, token);
        Assert.Equal(("tuit.uz", 2, false), (preview!.Title, preview.MemberCount, preview.IsMember));

        var joined = await _service.JoinByInviteAsync(_outsider.Id, token);

        Assert.Null(joined.Error);
        Assert.Contains(group.Chat.Members, member => member.UserId == _outsider.Id && member.Role == ChatMemberRole.Member);
        var message = group.Chat.Messages.Last();
        Assert.Equal(MessageServiceAction.MemberJoinedViaInvite, message.ServiceAction);
        Assert.Equal(_outsider.Id, message.SenderId);
        Assert.Null(joined.Group!.InviteToken);
    }

    [Fact]
    public async Task InviteLink_JoiningTwice_IsIdempotent()
    {
        var group = OrganizationGroup();
        var token = (await _service.CreateInviteLinkAsync(_admin.Id, group.ChatId)).Group!.InviteToken!;

        await _service.JoinByInviteAsync(_outsider.Id, token);
        var again = await _service.JoinByInviteAsync(_outsider.Id, token);

        Assert.Null(again.Error);
        Assert.Single(group.Chat.Members, member => member.UserId == _outsider.Id);
        Assert.Single(group.Chat.Messages);
    }

    [Fact]
    public async Task InviteLink_RegeneratedOrRevoked_OldLinkStopsWorking()
    {
        var group = OrganizationGroup();
        var first = (await _service.CreateInviteLinkAsync(_admin.Id, group.ChatId)).Group!.InviteToken!;
        var second = (await _service.CreateInviteLinkAsync(_admin.Id, group.ChatId)).Group!.InviteToken!;

        Assert.NotEqual(first, second);
        Assert.NotNull((await _service.JoinByInviteAsync(_outsider.Id, first)).Error);
        Assert.Null(await _service.GetInvitePreviewAsync(_outsider.Id, first));

        await _service.RevokeInviteLinkAsync(_admin.Id, group.ChatId);
        Assert.NotNull((await _service.JoinByInviteAsync(_outsider.Id, second)).Error);
        Assert.DoesNotContain(group.Chat.Members, member => member.UserId == _outsider.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaa!")]
    public async Task InviteLink_MalformedToken_IsRejected(string token)
    {
        OrganizationGroup().InviteLink = token;

        Assert.Null(await _service.GetInvitePreviewAsync(_outsider.Id, token));
        Assert.NotNull((await _service.JoinByInviteAsync(_outsider.Id, token)).Error);
    }

    [Fact]
    public async Task WithoutLinkOrAdmin_OutsiderHasNoWayIn()
    {
        var group = OrganizationGroup();

        Assert.Null(await _service.GetAsync(_outsider.Id, group.ChatId));
        Assert.False(await _service.JoinViaOrganizationAsync(group.ChatId, _outsider.Id));
        Assert.DoesNotContain(group.Chat.Members, member => member.UserId == _outsider.Id);
    }

    [Fact]
    public async Task JoinViaOrganization_DomainMember_IsAdded()
    {
        var group = OrganizationGroup();

        Assert.True(await _service.JoinViaOrganizationAsync(group.ChatId, _colleague.Id));
        Assert.Contains(group.Chat.Members, member => member.UserId == _colleague.Id);
    }
}
