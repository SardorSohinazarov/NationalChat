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

public sealed class OrganizationGroupTests
{
    private readonly FakeGroupRepository _repository = new();
    private readonly GroupService _service;
    private readonly Organization _tatu = TestData.Organization(1, "TATU", "tuit.uz");
    private readonly User _rektor;
    private readonly User _ali;
    private readonly User _vali;
    private readonly User _outsider = TestData.User(4, "Outsider");

    public OrganizationGroupTests()
    {
        _rektor = TestData.User(1, "Rektor").Verified(_tatu, OrganizationRole.Admin);
        _ali = TestData.User(2, "Ali").Verified(_tatu);
        _vali = TestData.User(3, "Vali").Verified(_tatu);
        _repository.Users.AddRange([_rektor, _ali, _vali, _outsider]);
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

    [Fact]
    public async Task Create_OrganizationOnly_SetsOrganizationAndBadges()
    {
        var result = await _service.CreateAsync(_ali.Id, new CreateGroupRequest("Kafedra", null, [_vali.Id], OrganizationOnly: true));

        Assert.Null(result.Error);
        Assert.Equal("TATU", result.Group!.Organization!.ShortName);
        Assert.False(result.Group.AutoJoin);
        Assert.All(result.Group.Members, member => Assert.Equal(_tatu.Id, member.Organization!.Id));
        Assert.Equal(_tatu.Id, Assert.Single(_repository.Groups).OrganizationId);
    }

    [Fact]
    public async Task Create_OrganizationOnly_ByNonMember_IsRejected()
    {
        var result = await _service.CreateAsync(_outsider.Id, new CreateGroupRequest("Kafedra", null, [_ali.Id], OrganizationOnly: true));

        Assert.Null(result.Group);
        Assert.Empty(_repository.Groups);
    }

    [Fact]
    public async Task Create_OrganizationOnly_WithOutsider_IsRejected()
    {
        var result = await _service.CreateAsync(_ali.Id, new CreateGroupRequest("Kafedra", null, [_vali.Id, _outsider.Id], OrganizationOnly: true));

        Assert.Equal("Bu guruhga faqat TATU a'zolarini qo'shish mumkin.", result.Error);
        Assert.Empty(_repository.Groups);
    }

    [Fact]
    public async Task Create_AutoJoin_ByNonAdminMember_IsRejected()
    {
        var result = await _service.CreateAsync(_ali.Id, new CreateGroupRequest("TATU — umumiy", null, [], OrganizationOnly: true, AutoJoin: true));

        Assert.Equal("Avtomatik qo'shishni faqat TATU admini yoqa oladi.", result.Error);
        Assert.Empty(_repository.Groups);
    }

    [Fact]
    public async Task Create_AutoJoin_ByAdmin_AddsAllCurrentMembers()
    {
        var result = await _service.CreateAsync(_rektor.Id, new CreateGroupRequest("TATU — umumiy", null, [], OrganizationOnly: true, AutoJoin: true));

        Assert.Null(result.Error);
        Assert.True(result.Group!.AutoJoin);
        Assert.Equal([_rektor.Id, _ali.Id, _vali.Id], result.Group.Members.Select(member => member.Id).Order());
    }

    [Fact]
    public async Task AddMembers_Outsider_IsRejectedWithOrganizationError()
    {
        var group = OrganizationGroup();

        var result = await _service.AddMembersAsync(_rektor.Id, group.ChatId, new AddGroupMembersRequest([_outsider.Id]));

        Assert.Equal("Bu guruhga faqat TATU a'zolarini qo'shish mumkin.", result.Error);
        Assert.DoesNotContain(group.Chat.Members, member => member.UserId == _outsider.Id);
    }

    [Fact]
    public async Task AddMembers_OrganizationMember_IsAdded()
    {
        var group = OrganizationGroup();

        var result = await _service.AddMembersAsync(_rektor.Id, group.ChatId, new AddGroupMembersRequest([_vali.Id]));

        Assert.Null(result.Error);
        Assert.Contains(group.Chat.Members, member => member.UserId == _vali.Id);
    }

    [Fact]
    public async Task AddMembers_OrdinaryGroup_StillAcceptsAnyone()
    {
        var group = TestData.Group(30, "Do'stlar", (_ali, ChatMemberRole.Creator));
        _repository.Groups.Add(group);

        var result = await _service.AddMembersAsync(_ali.Id, group.ChatId, new AddGroupMembersRequest([_outsider.Id]));

        Assert.Null(result.Error);
        Assert.Contains(group.Chat.Members, member => member.UserId == _outsider.Id);
    }

    [Fact]
    public async Task JoinViaOrganization_Outsider_IsNotAdded()
    {
        var group = OrganizationGroup(autoJoin: true);

        Assert.False(await _service.JoinViaOrganizationAsync(group.ChatId, _outsider.Id));
        Assert.DoesNotContain(group.Chat.Members, member => member.UserId == _outsider.Id);
    }

    [Fact]
    public async Task JoinViaOrganization_ExistingMember_IsNoOp()
    {
        var group = OrganizationGroup(autoJoin: true);

        Assert.False(await _service.JoinViaOrganizationAsync(group.ChatId, _ali.Id));
        Assert.Empty(group.Chat.Messages);
    }

    /// <summary>Rektor — ega, Ali — a'zo.</summary>
    private Group OrganizationGroup(bool autoJoin = false)
    {
        var group = TestData.Group(20, "TATU — kafedra", (_rektor, ChatMemberRole.Creator), (_ali, ChatMemberRole.Member));
        group.OrganizationId = _tatu.Id;
        group.Organization = _tatu;
        group.AutoJoin = autoJoin;
        _repository.Groups.Add(group);
        return group;
    }
}
