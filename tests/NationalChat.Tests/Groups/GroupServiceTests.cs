using Application.Features.Files;
using Application.Features.Groups;
using Application.Features.Groups.DataTransferObjects.Requests;
using Application.Features.Groups.Validators;
using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Responses;
using Application.Features.Presence;
using Domain.Entities;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.Groups;

public sealed class GroupServiceTests
{
    private readonly FakeGroupRepository _repository = new();
    private readonly IChatRealtimeNotifier _notifier = Substitute.For<IChatRealtimeNotifier>();
    private readonly GroupService _service;

    private readonly User _sardor = TestData.User(1, "Sardor");
    private readonly User _jasur = TestData.User(2, "Jasur");
    private readonly User _dilnoza = TestData.User(3, "Dilnoza");
    private readonly User _bekzod = TestData.User(4, "Bekzod");
    private readonly User _malika = TestData.User(5, "Malika");

    public GroupServiceTests()
    {
        _repository.Users.AddRange([_sardor, _jasur, _dilnoza, _bekzod, _malika]);
        _service = new GroupService(
            _repository,
            Substitute.For<IFileService>(),
            _notifier,
            Substitute.For<IPresenceTracker>(),
            new CreateGroupRequestValidator(),
            new UpdateGroupRequestValidator(),
            new AddGroupMembersRequestValidator(),
            new UpdateGroupMemberRoleRequestValidator(),
            new FixedTimeProvider(new DateTimeOffset(TestData.Start.AddDays(1))));
    }

    /// <summary>Sardor — ega, Jasur — admin, Dilnoza — a’zo.</summary>
    private Group SeedGroup()
    {
        var group = TestData.Group(10, "315-21 guruh",
            (_sardor, ChatMemberRole.Creator),
            (_jasur, ChatMemberRole.Admin),
            (_dilnoza, ChatMemberRole.Member));
        _repository.Groups.Add(group);
        return group;
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesGroupWithCreatorRoleAndServiceMessage()
    {
        var result = await _service.CreateAsync(_sardor.Id, new CreateGroupRequest("315-21 guruh", "TATU", [_jasur.Id, _dilnoza.Id]));

        Assert.Null(result.Error);
        Assert.NotNull(result.Group);
        Assert.Equal(ChatMemberRole.Creator, result.Group!.MyRole);
        Assert.Equal(3, result.Group.Members.Count);

        var chat = Assert.Single(_repository.Groups).Chat;
        Assert.Equal(ChatType.Group, chat.Type);
        var message = Assert.Single(chat.Messages);
        Assert.Equal(MessageServiceAction.GroupCreated, message.ServiceAction);

        await _notifier.Received(1).MessageCreatedAsync(
            Arg.Is<MessageDto>(m => m.ServiceAction == MessageServiceAction.GroupCreated),
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_UnknownMember_IsRejected()
    {
        var result = await _service.CreateAsync(_sardor.Id, new CreateGroupRequest("Guruh", null, [_jasur.Id, 999]));

        Assert.Null(result.Group);
        Assert.NotNull(result.Error);
        Assert.Empty(_repository.Groups);
    }

    [Fact]
    public async Task Create_EmptyTitle_FailsValidation()
    {
        var result = await _service.CreateAsync(_sardor.Id, new CreateGroupRequest("   ", null, [_jasur.Id]));

        Assert.Null(result.Group);
        Assert.Empty(_repository.Groups);
    }

    [Fact]
    public async Task AddMembers_ByRegularMember_IsForbidden()
    {
        var group = SeedGroup();

        var result = await _service.AddMembersAsync(_dilnoza.Id, group.ChatId, new AddGroupMembersRequest([_bekzod.Id]));

        Assert.Null(result.Group);
        Assert.Equal(3, group.Chat.Members.Count);
    }

    [Fact]
    public async Task AddMembers_ByAdmin_AddsMemberAndWritesServiceMessage()
    {
        var group = SeedGroup();

        var result = await _service.AddMembersAsync(_jasur.Id, group.ChatId, new AddGroupMembersRequest([_bekzod.Id, _malika.Id]));

        Assert.NotNull(result.Group);
        Assert.Equal(5, group.Chat.Members.Count);
        var message = Assert.Single(group.Chat.Messages);
        Assert.Equal(MessageServiceAction.MembersAdded, message.ServiceAction);
        Assert.Equal("Bekzod, Malika", message.TextContent);
        await _notifier.Received(1).GroupUpdatedAsync(group.ChatId, group.Title, group.Description, group.PhotoId, 5,
            Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddMembers_AlreadyInGroup_IsRejected()
    {
        var group = SeedGroup();

        var result = await _service.AddMembersAsync(_sardor.Id, group.ChatId, new AddGroupMembersRequest([_dilnoza.Id]));

        Assert.Null(result.Group);
        Assert.Empty(group.Chat.Messages);
    }

    [Fact]
    public async Task AddMembers_OverMemberLimit_IsRejected()
    {
        var members = new List<(User, ChatMemberRole)> { (_sardor, ChatMemberRole.Creator) };
        for (var id = 100; id < 100 + GroupLimits.MaxMembers - 1; id++) members.Add((TestData.User(id, $"U{id}"), ChatMemberRole.Member));
        var group = TestData.Group(20, "Katta guruh", [.. members]);
        _repository.Groups.Add(group);

        var result = await _service.AddMembersAsync(_sardor.Id, group.ChatId, new AddGroupMembersRequest([_bekzod.Id]));

        Assert.Null(result.Group);
        Assert.Equal(GroupLimits.MaxMembers, group.Chat.Members.Count);
    }

    [Fact]
    public async Task RemoveMember_AdminCannotRemoveCreator()
    {
        var group = SeedGroup();

        var result = await _service.RemoveMemberAsync(_jasur.Id, group.ChatId, _sardor.Id);

        Assert.Null(result.Group);
        Assert.Contains(group.Chat.Members, m => m.UserId == _sardor.Id);
    }

    [Fact]
    public async Task RemoveMember_ByCreator_RemovesAndNotifiesRemovedUser()
    {
        var group = SeedGroup();

        var result = await _service.RemoveMemberAsync(_sardor.Id, group.ChatId, _jasur.Id);

        Assert.NotNull(result.Group);
        Assert.DoesNotContain(group.Chat.Members, m => m.UserId == _jasur.Id);
        Assert.Equal(MessageServiceAction.MemberRemoved, Assert.Single(group.Chat.Messages).ServiceAction);
        await _notifier.Received(1).ChatDeletedAsync(group.ChatId,
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.Single() == _jasur.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRole_OnlyCreatorCanPromote()
    {
        var group = SeedGroup();

        var byAdmin = await _service.UpdateMemberRoleAsync(_jasur.Id, group.ChatId, _dilnoza.Id, new UpdateGroupMemberRoleRequest(ChatMemberRole.Admin));
        var byCreator = await _service.UpdateMemberRoleAsync(_sardor.Id, group.ChatId, _dilnoza.Id, new UpdateGroupMemberRoleRequest(ChatMemberRole.Admin));

        Assert.Null(byAdmin.Group);
        Assert.NotNull(byCreator.Group);
        Assert.Equal(ChatMemberRole.Admin, group.Chat.Members.Single(m => m.UserId == _dilnoza.Id).Role);
    }

    [Fact]
    public async Task UpdateRole_CreatorRoleCannotBeGranted()
    {
        var group = SeedGroup();

        var result = await _service.UpdateMemberRoleAsync(_sardor.Id, group.ChatId, _dilnoza.Id, new UpdateGroupMemberRoleRequest(ChatMemberRole.Creator));

        Assert.Null(result.Group);
        Assert.Equal(ChatMemberRole.Member, group.Chat.Members.Single(m => m.UserId == _dilnoza.Id).Role);
    }

    [Fact]
    public async Task Update_TitleChange_WritesTitleChangedServiceMessage()
    {
        var group = SeedGroup();

        var result = await _service.UpdateAsync(_jasur.Id, group.ChatId, new UpdateGroupRequest("Diplom jamoasi", "Yangi tavsif"));

        Assert.NotNull(result.Group);
        Assert.Equal("Diplom jamoasi", group.Title);
        var message = Assert.Single(group.Chat.Messages);
        Assert.Equal(MessageServiceAction.TitleChanged, message.ServiceAction);
        Assert.Equal("Diplom jamoasi", message.TextContent);
    }

    [Fact]
    public async Task Leave_Creator_TransfersOwnershipToOldestAdmin()
    {
        // Bekzod joined before the admins, but admins take precedence over regular members.
        var group = TestData.Group(30, "Guruh",
            (_sardor, ChatMemberRole.Creator),
            (_bekzod, ChatMemberRole.Member),
            (_jasur, ChatMemberRole.Admin),
            (_malika, ChatMemberRole.Admin));
        _repository.Groups.Add(group);

        var result = await _service.LeaveAsync(_sardor.Id, group.ChatId);

        Assert.True(result.Succeeded);
        Assert.Equal(_jasur.Id, group.CreatorId);
        Assert.Equal(ChatMemberRole.Creator, group.Chat.Members.Single(m => m.UserId == _jasur.Id).Role);
        Assert.Equal(MessageServiceAction.MemberLeft, Assert.Single(group.Chat.Messages).ServiceAction);
    }

    [Fact]
    public async Task Leave_LastMember_DeletesChat()
    {
        var group = TestData.Group(40, "Yolg‘iz", (_sardor, ChatMemberRole.Creator));
        _repository.Groups.Add(group);

        var result = await _service.LeaveAsync(_sardor.Id, group.ChatId);

        Assert.True(result.Succeeded);
        Assert.NotNull(group.Chat.DeletedAt);
        Assert.Empty(group.Chat.Messages);
    }

    [Fact]
    public async Task Get_NonMember_ReturnsNull()
    {
        var group = SeedGroup();

        Assert.Null(await _service.GetAsync(_bekzod.Id, group.ChatId));
        Assert.NotNull(await _service.GetAsync(_dilnoza.Id, group.ChatId));
    }
}
