using Application.Features.SecretChats;
using Application.Features.SecretChats.DataTransferObjects.Requests;
using Application.Features.SecretChats.DataTransferObjects.Responses;
using Application.Features.SecretChats.Validators;
using Domain.Entities;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.SecretChats;

public sealed class SecretChatServiceTests
{
    private const int AliLaptop = 11;
    private const int AliPhone = 12;
    private const int ValiLaptop = 21;
    private const int ValiPhone = 22;
    private const int GuliPhone = 31;

    private static readonly DateTime Now = TestData.Start.AddDays(1);
    private static readonly string AliKey = Key(1);
    private static readonly string ValiKey = Key(2);

    private readonly FakeSecretChatRepository _repository = new();
    private readonly ISecretChatRealtimeNotifier _notifier = Substitute.For<ISecretChatRealtimeNotifier>();
    private readonly SecretChatService _service;

    private readonly User _ali = TestData.User(1, "Ali");
    private readonly User _vali = TestData.User(2, "Vali");
    private readonly User _guli = TestData.User(3, "Guli");

    public SecretChatServiceTests()
    {
        _repository.Users.AddRange([_ali, _vali, _guli]);
        _repository.Sessions.AddRange([
            Session(AliLaptop, _ali), Session(AliPhone, _ali),
            Session(ValiLaptop, _vali), Session(ValiPhone, _vali),
            Session(GuliPhone, _guli)]);
        _service = CreateService(Now);
    }

    private SecretChatService CreateService(DateTime now) => new(
        _repository,
        _notifier,
        new CreateSecretChatRequestValidator(),
        new AcceptSecretChatRequestValidator(),
        new SendSecretMessageRequestValidator(),
        new AckSecretMessagesRequestValidator(),
        new SecretMessagesQueryValidator(),
        new FixedTimeProvider(new DateTimeOffset(now)));

    private static Session Session(int id, User user) => new()
    {
        Id = id,
        UserId = user.Id,
        User = user,
        CreatedAt = TestData.Start,
        LastActiveAt = TestData.Start,
        ExpiresAt = Now.AddDays(30),
    };

    private static string Key(byte fill) => Convert.ToBase64String(Enumerable.Repeat(fill, SecretChatLimits.PublicKeyBytes).ToArray());

    private static string Cipher(int length = 48) => Convert.ToBase64String(Enumerable.Repeat((byte)7, length).ToArray());

    /// <summary>Ali (laptop) asks Vali; Vali accepts on the phone.</summary>
    private async Task<SecretChatDto> ActiveChatAsync()
    {
        var created = await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey));
        var accepted = await _service.AcceptAsync(_vali.Id, ValiPhone, created.Chat!.Id, new AcceptSecretChatRequest(ValiKey));
        Assert.Null(accepted.Error);
        _notifier.ClearReceivedCalls();
        return accepted.Chat!;
    }

    [Fact]
    public async Task Create_BindsInitiatorDevice_AndNotifiesEveryParticipantDevice()
    {
        var result = await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey));

        Assert.Null(result.Error);
        var chat = result.Chat!;
        Assert.Equal(SecretChatStatus.Pending, chat.Status);
        Assert.True(chat.IsInitiator);
        Assert.Equal(AliLaptop, chat.MySessionId);
        Assert.Equal(AliKey, chat.MyPublicKey);
        Assert.Null(chat.PeerPublicKey);
        Assert.Equal(_vali.Id, chat.Peer.Id);

        await _notifier.Received(1).RequestedAsync(
            Arg.Is<SecretChatDto>(dto => !dto.IsInitiator && dto.MySessionId == null && dto.PeerPublicKey == AliKey && dto.Peer.Id == _ali.Id),
            _vali.Id,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-base64!")]
    [InlineData("AAAA")]
    public async Task Create_InvalidPublicKey_IsRejected(string publicKey)
    {
        var result = await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, publicKey));

        Assert.Null(result.Chat);
        Assert.Empty(_repository.Chats);
    }

    [Fact]
    public async Task Create_WithSelf_IsRejected()
    {
        var result = await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_ali.Id, AliKey));

        Assert.Null(result.Chat);
        Assert.Empty(_repository.Chats);
    }

    [Fact]
    public async Task Create_UnknownUser_IsRejected()
    {
        var result = await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(999, AliKey));

        Assert.Null(result.Chat);
    }

    [Fact]
    public async Task Create_SecondPendingRequestToSamePerson_IsRejected()
    {
        await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey));

        var again = await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey));

        Assert.Null(again.Chat);
        Assert.Single(_repository.Chats);
    }

    [Fact]
    public async Task Create_FromRevokedSession_IsRejected()
    {
        _repository.Sessions.Single(s => s.Id == AliLaptop).RevokedAt = Now.AddMinutes(-1);

        var result = await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey));

        Assert.Null(result.Chat);
        Assert.Empty(_repository.Chats);
    }

    [Fact]
    public async Task Create_WithSessionOfAnotherUser_IsRejected()
    {
        var result = await _service.CreateAsync(_ali.Id, ValiLaptop, new CreateSecretChatRequest(_guli.Id, AliKey));

        Assert.Null(result.Chat);
    }

    [Fact]
    public async Task PendingRequest_IsVisibleOnEveryParticipantDevice_ButOnlyOnTheInitiatingDevice()
    {
        var chat = (await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey))).Chat!;

        Assert.Single(await _service.ListAsync(_ali.Id, AliLaptop));
        Assert.Empty(await _service.ListAsync(_ali.Id, AliPhone));
        Assert.Null(await _service.GetAsync(_ali.Id, AliPhone, chat.Id));
        Assert.Single(await _service.ListAsync(_vali.Id, ValiLaptop));
        Assert.Single(await _service.ListAsync(_vali.Id, ValiPhone));
        Assert.Empty(await _service.ListAsync(_guli.Id, GuliPhone));
    }

    [Fact]
    public async Task Accept_BindsTheAcceptingDevice_AndOtherDevicesLoseAccess()
    {
        var created = (await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey))).Chat!;

        var result = await _service.AcceptAsync(_vali.Id, ValiPhone, created.Id, new AcceptSecretChatRequest(ValiKey));

        Assert.Null(result.Error);
        Assert.Equal(SecretChatStatus.Active, result.Chat!.Status);
        Assert.Equal(ValiPhone, result.Chat.MySessionId);
        Assert.Equal(AliKey, result.Chat.PeerPublicKey);
        Assert.Null(await _service.GetAsync(_vali.Id, ValiLaptop, created.Id));
        Assert.Empty(await _service.ListAsync(_vali.Id, ValiLaptop));

        var initiatorView = await _service.GetAsync(_ali.Id, AliLaptop, created.Id);
        Assert.Equal(ValiKey, initiatorView!.PeerPublicKey);

        await _notifier.Received(1).AcceptedAsync(
            Arg.Is<SecretChatDto>(dto => dto.IsInitiator && dto.PeerPublicKey == ValiKey),
            AliLaptop,
            Arg.Is<SecretChatDto>(dto => !dto.IsInitiator && dto.MySessionId == ValiPhone),
            _vali.Id,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Accept_ByInitiator_IsRejected()
    {
        var created = (await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey))).Chat!;

        var result = await _service.AcceptAsync(_ali.Id, AliLaptop, created.Id, new AcceptSecretChatRequest(ValiKey));

        Assert.Null(result.Chat);
        Assert.Equal(SecretChatStatus.Pending, _repository.Chats.Single().Status);
    }

    [Fact]
    public async Task Accept_Twice_IsRejectedAndKeepsTheFirstDevice()
    {
        var chat = await ActiveChatAsync();

        var again = await _service.AcceptAsync(_vali.Id, ValiPhone, chat.Id, new AcceptSecretChatRequest(Key(9)));

        Assert.Null(again.Chat);
        Assert.Equal(ValiKey, _repository.Chats.Single().ParticipantPublicKey);
    }

    [Fact]
    public async Task Send_BeforeAccept_IsRejected()
    {
        var created = (await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey))).Chat!;

        var result = await _service.SendAsync(_ali.Id, AliLaptop, created.Id, new SendSecretMessageRequest(1, Cipher()));

        Assert.Null(result.Message);
        Assert.Empty(_repository.Messages);
    }

    [Fact]
    public async Task Send_IsQueuedAndPushedOnlyToThePeerDevice()
    {
        var chat = await ActiveChatAsync();

        var result = await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher()));

        Assert.Null(result.Error);
        Assert.Equal(Cipher(), result.Message!.Ciphertext);
        await _notifier.Received(1).MessageReceivedAsync(Arg.Is<SecretMessageDto>(m => m.Seq == 1), ValiPhone, Arg.Any<CancellationToken>());

        var peerQueue = await _service.GetMessagesAsync(_vali.Id, ValiPhone, chat.Id, new SecretMessagesQuery());
        Assert.Single(peerQueue.Page!.Items);
        var ownQueue = await _service.GetMessagesAsync(_ali.Id, AliLaptop, chat.Id, new SecretMessagesQuery());
        Assert.Empty(ownQueue.Page!.Items);
        var otherDevice = await _service.GetMessagesAsync(_vali.Id, ValiLaptop, chat.Id, new SecretMessagesQuery());
        Assert.Null(otherDevice.Page);
    }

    [Fact]
    public async Task Send_FromAnUnboundDevice_IsRejected()
    {
        var chat = await ActiveChatAsync();

        var fromAliPhone = await _service.SendAsync(_ali.Id, AliPhone, chat.Id, new SendSecretMessageRequest(1, Cipher()));
        var fromValiLaptop = await _service.SendAsync(_vali.Id, ValiLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher()));
        var fromGuli = await _service.SendAsync(_guli.Id, GuliPhone, chat.Id, new SendSecretMessageRequest(1, Cipher()));

        Assert.Null(fromAliPhone.Message);
        Assert.Null(fromValiLaptop.Message);
        Assert.Null(fromGuli.Message);
        Assert.Empty(_repository.Messages);
    }

    [Fact]
    public async Task Send_ReplayedOrOlderSeq_IsRejected_PerSide()
    {
        var chat = await ActiveChatAsync();

        Assert.NotNull((await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher()))).Message);
        Assert.Null((await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher()))).Message);
        Assert.NotNull((await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(3, Cipher()))).Message);
        Assert.Null((await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(2, Cipher()))).Message);
        Assert.Null((await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(0, Cipher()))).Message);

        // Each side has its own counter.
        Assert.NotNull((await _service.SendAsync(_vali.Id, ValiPhone, chat.Id, new SendSecretMessageRequest(1, Cipher()))).Message);
        Assert.Equal(3, _repository.Messages.Count);
    }

    [Fact]
    public async Task Send_OversizedOrEmptyCiphertext_IsRejected()
    {
        var chat = await ActiveChatAsync();

        var tooBig = await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher(SecretChatLimits.MaxCiphertextBytes + 1)));
        var empty = await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, ""));

        Assert.Null(tooBig.Message);
        Assert.Null(empty.Message);
        Assert.Empty(_repository.Messages);
    }

    [Fact]
    public async Task GetMessages_PagesTheQueueOldestFirst()
    {
        var chat = await ActiveChatAsync();
        for (var seq = 1; seq <= 3; seq++)
        {
            await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(seq, Cipher()));
        }

        var first = (await _service.GetMessagesAsync(_vali.Id, ValiPhone, chat.Id, new SecretMessagesQuery { Limit = 2 })).Page!;
        Assert.Equal([1L, 2L], first.Items.Select(m => m.Seq));
        Assert.True(first.HasMore);

        var second = (await _service.GetMessagesAsync(_vali.Id, ValiPhone, chat.Id, new SecretMessagesQuery { Limit = 2, AfterId = first.NextCursor })).Page!;
        Assert.Equal([3L], second.Items.Select(m => m.Seq));
        Assert.False(second.HasMore);
        Assert.Null(second.NextCursor);
    }

    [Fact]
    public async Task Ack_DeletesOnlyDeliveredMessagesAddressedToThisDevice()
    {
        var chat = await ActiveChatAsync();
        for (var seq = 1; seq <= 3; seq++)
        {
            await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(seq, Cipher()));
        }
        await _service.SendAsync(_vali.Id, ValiPhone, chat.Id, new SendSecretMessageRequest(1, Cipher()));
        var second = _repository.Messages.Single(m => m.SenderSessionId == AliLaptop && m.Seq == 2);

        var result = await _service.AckAsync(_vali.Id, ValiPhone, chat.Id, new AckSecretMessagesRequest(second.Id));

        Assert.True(result.Succeeded);
        Assert.Equal([3L], _repository.Messages.Where(m => m.SenderSessionId == AliLaptop).Select(m => m.Seq));
        Assert.Single(_repository.Messages, m => m.SenderSessionId == ValiPhone);
    }

    [Fact]
    public async Task Ack_FromAnotherDevice_IsRejected()
    {
        var chat = await ActiveChatAsync();
        await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher()));

        var result = await _service.AckAsync(_vali.Id, ValiLaptop, chat.Id, new AckSecretMessagesRequest(long.MaxValue));

        Assert.False(result.Succeeded);
        Assert.Single(_repository.Messages);
    }

    [Fact]
    public async Task Close_DropsTheQueue_NotifiesBothDevices_AndStopsSending()
    {
        var chat = await ActiveChatAsync();
        await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher()));

        var result = await _service.CloseAsync(_vali.Id, ValiPhone, chat.Id);

        Assert.Equal(SecretChatStatus.Closed, result.Chat!.Status);
        Assert.Equal(Now, result.Chat.ClosedAt);
        Assert.Empty(_repository.Messages);
        await _notifier.Received(1).ClosedAsync(
            chat.Id,
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.Order().SequenceEqual(new[] { AliLaptop, ValiPhone })),
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 0),
            Arg.Any<CancellationToken>());
        Assert.Null((await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(2, Cipher()))).Message);
        Assert.Empty(await _service.ListAsync(_ali.Id, AliLaptop));
    }

    [Fact]
    public async Task Decline_PendingRequestFromAnyDevice_NotifiesAllParticipantDevices()
    {
        var created = (await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey))).Chat!;

        var result = await _service.CloseAsync(_vali.Id, ValiLaptop, created.Id);

        Assert.Equal(SecretChatStatus.Closed, result.Chat!.Status);
        await _notifier.Received(1).ClosedAsync(
            created.Id,
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { AliLaptop })),
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { _vali.Id })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Outsider_CannotSeeAcceptOrClose()
    {
        var created = (await _service.CreateAsync(_ali.Id, AliLaptop, new CreateSecretChatRequest(_vali.Id, AliKey))).Chat!;

        Assert.Null(await _service.GetAsync(_guli.Id, GuliPhone, created.Id));
        Assert.Null((await _service.AcceptAsync(_guli.Id, GuliPhone, created.Id, new AcceptSecretChatRequest(Key(3)))).Chat);
        Assert.Null((await _service.CloseAsync(_guli.Id, GuliPhone, created.Id)).Chat);
        Assert.Equal(SecretChatStatus.Pending, _repository.Chats.Single().Status);
    }

    [Fact]
    public async Task CloseForSessions_ClosesOnlyChatsBoundToThoseDevices()
    {
        var chat = await ActiveChatAsync();
        var other = (await _service.CreateAsync(_ali.Id, AliPhone, new CreateSecretChatRequest(_guli.Id, AliKey))).Chat!;
        await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher()));

        await _service.CloseForSessionsAsync([ValiPhone]);

        Assert.Equal(SecretChatStatus.Closed, _repository.Chats.Single(c => c.Id == chat.Id).Status);
        Assert.Equal(SecretChatStatus.Pending, _repository.Chats.Single(c => c.Id == other.Id).Status);
        Assert.Empty(_repository.Messages);
    }

    [Fact]
    public async Task Cleanup_ClosesChatsOfExpiredSessionsAndStaleRequests_AndDropsOldCiphertext()
    {
        var active = await ActiveChatAsync();
        await _service.SendAsync(_ali.Id, AliLaptop, active.Id, new SendSecretMessageRequest(1, Cipher()));
        var pending = (await _service.CreateAsync(_ali.Id, AliPhone, new CreateSecretChatRequest(_guli.Id, AliKey))).Chat!;
        var healthy = (await _service.CreateAsync(_guli.Id, GuliPhone, new CreateSecretChatRequest(_ali.Id, Key(3)))).Chat!;
        var healthyChat = _repository.Chats.Single(c => c.Id == healthy.Id);
        healthyChat.CreatedAt = Now.AddDays(8) - SecretChatLimits.PendingLifetime + TimeSpan.FromHours(1);

        // Eight days later: Vali's phone session expired and Ali's request to Guli was never answered.
        var later = Now.AddDays(8);
        _repository.Sessions.Single(s => s.Id == ValiPhone).ExpiresAt = later.AddMinutes(-1);
        await CreateService(later).CleanupAsync();

        Assert.Equal(SecretChatStatus.Closed, _repository.Chats.Single(c => c.Id == active.Id).Status);
        Assert.Equal(SecretChatStatus.Closed, _repository.Chats.Single(c => c.Id == pending.Id).Status);
        Assert.Equal(SecretChatStatus.Pending, healthyChat.Status);
        Assert.Empty(_repository.Messages);
    }

    [Fact]
    public async Task Cleanup_DropsCiphertextPastRetention_EvenInOpenChats()
    {
        var chat = await ActiveChatAsync();
        await _service.SendAsync(_ali.Id, AliLaptop, chat.Id, new SendSecretMessageRequest(1, Cipher()));

        await CreateService(Now + SecretChatLimits.UndeliveredRetention + TimeSpan.FromMinutes(1)).CleanupAsync();

        Assert.Equal(SecretChatStatus.Active, _repository.Chats.Single().Status);
        Assert.Empty(_repository.Messages);
    }
}
