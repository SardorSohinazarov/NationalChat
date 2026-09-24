using Application.Features.Chats;
using Application.Features.Chats.DataTransferObjects.Requests;
using Application.Features.Chats.Validators;
using Application.Features.Messages;
using Application.Features.Messages.Validators;
using Application.Features.Presence;
using Domain.Entities;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.Chats;

public sealed class ChatPermissionTests
{
    private static readonly FixedTimeProvider Clock = new(new DateTimeOffset(TestData.Start));

    private static ChatMember Membership(ChatType type, ChatMemberRole role) =>
        new() { ChatId = 7, UserId = 1, Role = role, Chat = new Chat { Id = 7, Type = type } };

    private static ChatService CreateChatService(IChatRepository repository) =>
        new(repository, Substitute.For<IChatRealtimeNotifier>(), Substitute.For<IPresenceTracker>(), new CreatePrivateChatRequestValidator(), Clock);

    [Fact]
    public async Task DeleteGroup_ByNonCreator_IsDenied()
    {
        var repository = Substitute.For<IChatRepository>();
        repository.FindMembershipAsync(7, 1, Arg.Any<CancellationToken>()).Returns(Membership(ChatType.Group, ChatMemberRole.Admin));

        var deleted = await CreateChatService(repository).DeleteAsync(1, 7);

        Assert.False(deleted);
        await repository.DidNotReceiveWithAnyArgs().SoftDeleteAsync(default, default, default);
    }

    [Theory]
    [InlineData(ChatType.Group, ChatMemberRole.Creator)]
    [InlineData(ChatType.Private, ChatMemberRole.Member)]
    public async Task Delete_ByCreatorOrPrivateParticipant_IsAllowed(ChatType type, ChatMemberRole role)
    {
        var repository = Substitute.For<IChatRepository>();
        repository.FindMembershipAsync(7, 1, Arg.Any<CancellationToken>()).Returns(Membership(type, role));
        repository.SoftDeleteAsync(7, 1, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(new[] { 1, 2 });

        Assert.True(await CreateChatService(repository).DeleteAsync(1, 7));
    }

    [Fact]
    public async Task ClearGroupHistory_ByRegularMember_IsDenied()
    {
        var repository = Substitute.For<IMessageRepository>();
        repository.FindMembershipAsync(7, 1, Arg.Any<CancellationToken>()).Returns(Membership(ChatType.Group, ChatMemberRole.Member));
        var service = new MessageService(repository, Substitute.For<IChatRealtimeNotifier>(),
            new SendMessageRequestValidator(), new UpdateMessageRequestValidator(), new MessageSearchRequestValidator(), Clock);

        Assert.False(await service.ClearChatAsync(1, 7));
        await repository.DidNotReceiveWithAnyArgs().ClearChatAsync(default, default, default);
    }
}
