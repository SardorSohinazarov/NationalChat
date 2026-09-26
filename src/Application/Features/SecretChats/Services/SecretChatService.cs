using Application.Features.SecretChats.DataTransferObjects.Requests;
using Application.Features.SecretChats.DataTransferObjects.Responses;
using Application.Features.SecretChats.Factories;
using Application.Features.SecretChats.Mappers;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.SecretChats;

public sealed class SecretChatService(
    ISecretChatRepository repository,
    ISecretChatRealtimeNotifier realtimeNotifier,
    ISecretFileStorage fileStorage,
    IValidator<CreateSecretChatRequest> createValidator,
    IValidator<AcceptSecretChatRequest> acceptValidator,
    IValidator<SendSecretMessageRequest> sendValidator,
    IValidator<AckSecretMessagesRequest> ackValidator,
    IValidator<SecretMessagesQuery> queryValidator,
    TimeProvider timeProvider) : ISecretChatService
{
    private const string NotFoundError = "Maxfiy chat topilmadi.";
    private const string InactiveSessionError = "Qurilma sessiyasi faol emas.";
    private const string DuplicateSeqError = "Bu tartib raqamli xabar allaqachon qabul qilingan.";
    private const int CleanupBatchSize = 200;

    public async Task<IReadOnlyList<SecretChatDto>> ListAsync(int userId, int sessionId, CancellationToken cancellationToken = default)
    {
        if (!await IsSessionActiveAsync(userId, sessionId, cancellationToken)) return [];

        var chats = await repository.GetVisibleAsync(userId, sessionId, cancellationToken);
        return chats.Select(chat => SecretChatMapper.ToDto(chat, userId)).ToList();
    }

    public async Task<SecretChatDto?> GetAsync(int userId, int sessionId, int secretChatId, CancellationToken cancellationToken = default)
    {
        if (!await IsSessionActiveAsync(userId, sessionId, cancellationToken)) return null;

        var chat = await repository.GetAsync(secretChatId, cancellationToken);
        return chat is null || SecretChatAccess.SideOf(chat, userId, sessionId) == SecretChatAccess.Side.None ? null : SecretChatMapper.ToDto(chat, userId);
    }

    public async Task<SecretChatResult> CreateAsync(int userId, int sessionId, CreateSecretChatRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return Fail(validation.Errors[0].ErrorMessage);
        if (request.ParticipantId == userId) return Fail("O'zingiz bilan maxfiy chat ochib bo'lmaydi.");
        if (!await IsSessionActiveAsync(userId, sessionId, cancellationToken)) return Fail(InactiveSessionError);

        var initiator = await repository.FindUserAsync(userId, cancellationToken);
        var participant = await repository.FindUserAsync(request.ParticipantId, cancellationToken);
        if (initiator is null || participant is null) return Fail("Foydalanuvchi topilmadi.");
        if (await repository.HasPendingRequestAsync(sessionId, participant.Id, cancellationToken))
        {
            return Fail("Bu foydalanuvchiga yuborilgan so'rov hali javob kutmoqda.");
        }

        var chat = SecretChatFactory.CreateRequest(userId, sessionId, participant.Id, request.PublicKey, Now());
        chat.Initiator = initiator;
        chat.Participant = participant;
        await repository.AddAsync(chat, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.RequestedAsync(SecretChatMapper.ToDto(chat, participant.Id), participant.Id, cancellationToken);
        return new(SecretChatMapper.ToDto(chat, userId), null);
    }

    public async Task<SecretChatResult> AcceptAsync(int userId, int sessionId, int secretChatId, AcceptSecretChatRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await acceptValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return Fail(validation.Errors[0].ErrorMessage);
        if (!await IsSessionActiveAsync(userId, sessionId, cancellationToken)) return Fail(InactiveSessionError);

        var chat = await repository.GetAsync(secretChatId, cancellationToken);
        if (chat is null || SecretChatAccess.SideOf(chat, userId, sessionId) == SecretChatAccess.Side.None) return Fail(NotFoundError);
        if (chat.ParticipantId != userId) return Fail("So'rovni faqat qabul qiluvchi tasdiqlaydi.");
        if (chat.Status != SecretChatStatus.Pending) return Fail("Bu so'rov allaqachon javob olgan.");

        chat.ParticipantSessionId = sessionId;
        chat.ParticipantPublicKey = request.PublicKey;
        chat.Status = SecretChatStatus.Active;
        chat.AcceptedAt = Now();
        await repository.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.AcceptedAsync(
            SecretChatMapper.ToDto(chat, chat.InitiatorId), chat.InitiatorSessionId,
            SecretChatMapper.ToDto(chat, chat.ParticipantId), chat.ParticipantId,
            cancellationToken);
        return new(SecretChatMapper.ToDto(chat, userId), null);
    }

    public async Task<SecretChatResult> CloseAsync(int userId, int sessionId, int secretChatId, CancellationToken cancellationToken = default)
    {
        if (!await IsSessionActiveAsync(userId, sessionId, cancellationToken)) return Fail(InactiveSessionError);

        var chat = await repository.GetAsync(secretChatId, cancellationToken);
        if (chat is null || SecretChatAccess.SideOf(chat, userId, sessionId) == SecretChatAccess.Side.None) return Fail(NotFoundError);

        if (chat.Status != SecretChatStatus.Closed)
        {
            await CloseChatsAsync([chat], cancellationToken);
        }

        return new(SecretChatMapper.ToDto(chat, userId), null);
    }

    public async Task<SecretMessageResult> SendAsync(int userId, int sessionId, int secretChatId, SendSecretMessageRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await sendValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return new(null, validation.Errors[0].ErrorMessage);
        if (!await IsSessionActiveAsync(userId, sessionId, cancellationToken)) return new(null, InactiveSessionError);

        var chat = await repository.GetAsync(secretChatId, cancellationToken);
        var side = chat is null ? SecretChatAccess.Side.None : SecretChatAccess.SideOf(chat, userId, sessionId);
        if (chat is null || side == SecretChatAccess.Side.None) return new(null, NotFoundError);
        if (chat.Status != SecretChatStatus.Active) return new(null, "Maxfiy chat faol emas.");

        var lastSeq = side == SecretChatAccess.Side.Initiator ? chat.InitiatorLastSeq : chat.ParticipantLastSeq;
        if (request.Seq <= lastSeq) return new(null, DuplicateSeqError, Duplicate: true);

        if (side == SecretChatAccess.Side.Initiator) chat.InitiatorLastSeq = request.Seq;
        else chat.ParticipantLastSeq = request.Seq;

        var ciphertext = SecretChatBase64.TryDecode(request.Ciphertext)!;
        var message = SecretChatFactory.CreateMessage(chat.Id, sessionId, request.Seq, ciphertext, Now());
        if (!await repository.TryAddMessageAsync(message, cancellationToken))
        {
            return new(null, DuplicateSeqError, Duplicate: true);
        }

        var dto = SecretChatMapper.ToDto(message);
        var recipientSessionId = side == SecretChatAccess.Side.Initiator ? chat.ParticipantSessionId!.Value : chat.InitiatorSessionId;
        await realtimeNotifier.MessageReceivedAsync(dto, recipientSessionId, cancellationToken);
        return new(dto, null);
    }

    public async Task<SecretMessagesResult> GetMessagesAsync(int userId, int sessionId, int secretChatId, SecretMessagesQuery query, CancellationToken cancellationToken = default)
    {
        var validation = await queryValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid) return new(null, validation.Errors[0].ErrorMessage);
        if (!await IsSessionActiveAsync(userId, sessionId, cancellationToken)) return new(null, InactiveSessionError);

        var chat = await repository.GetAsync(secretChatId, cancellationToken);
        if (chat is null || !SecretChatAccess.IsBound(chat, userId, sessionId)) return new(null, NotFoundError);

        var messages = await repository.GetQueueAsync(chat.Id, sessionId, query.AfterId, query.Limit + 1, cancellationToken);
        var hasMore = messages.Count > query.Limit;
        var items = messages.Take(query.Limit).Select(SecretChatMapper.ToDto).ToList();
        return new(new SecretMessagesPage(items, hasMore ? items[^1].Id : null, hasMore), null);
    }

    public async Task<SecretChatActionResult> AckAsync(int userId, int sessionId, int secretChatId, AckSecretMessagesRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ackValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return new(false, validation.Errors[0].ErrorMessage);
        if (!await IsSessionActiveAsync(userId, sessionId, cancellationToken)) return new(false, InactiveSessionError);

        var chat = await repository.GetAsync(secretChatId, cancellationToken);
        if (chat is null || !SecretChatAccess.IsBound(chat, userId, sessionId)) return new(false, NotFoundError);

        await repository.DeleteDeliveredAsync(chat.Id, sessionId, request.UpToId, cancellationToken);
        return new(true, null);
    }

    public async Task CloseForSessionsAsync(IReadOnlyCollection<int> sessionIds, CancellationToken cancellationToken = default)
    {
        if (sessionIds.Count == 0) return;

        var chats = await repository.GetOpenBySessionsAsync(sessionIds, cancellationToken);
        await CloseChatsAsync(chats, cancellationToken);
    }

    public async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        var now = Now();
        while (!cancellationToken.IsCancellationRequested)
        {
            var stale = await repository.GetStaleAsync(now, now - SecretChatLimits.PendingLifetime, CleanupBatchSize, cancellationToken);
            await CloseChatsAsync(stale, cancellationToken);
            if (stale.Count < CleanupBatchSize) break;
        }

        var cutoff = now - SecretChatLimits.UndeliveredRetention;
        await repository.DeleteUndeliveredBeforeAsync(cutoff, cancellationToken);
        await DeleteBlobsAsync(await repository.DeleteFilesCreatedBeforeAsync(cutoff, cancellationToken), cancellationToken);
    }

    private async Task CloseChatsAsync(IReadOnlyList<SecretChat> chats, CancellationToken cancellationToken)
    {
        if (chats.Count == 0) return;

        var now = Now();
        var fileIds = new List<Guid>();
        foreach (var chat in chats)
        {
            chat.Status = SecretChatStatus.Closed;
            chat.ClosedAt = now;
            await repository.DeleteMessagesAsync(chat.Id, cancellationToken);
            fileIds.AddRange(await repository.DeleteFilesOfChatAsync(chat.Id, cancellationToken));
        }

        await repository.SaveChangesAsync(cancellationToken);
        await DeleteBlobsAsync(fileIds, cancellationToken);

        foreach (var chat in chats)
        {
            // A pending request is not bound on the participant's side yet, so all of their devices hear about it.
            var sessionIds = chat.ParticipantSessionId is { } participantSessionId
                ? new[] { chat.InitiatorSessionId, participantSessionId }
                : new[] { chat.InitiatorSessionId };
            int[] userIds = chat.ParticipantSessionId is null ? [chat.ParticipantId] : [];
            await realtimeNotifier.ClosedAsync(chat.Id, sessionIds, userIds, cancellationToken);
        }
    }

    private async Task DeleteBlobsAsync(IReadOnlyList<Guid> fileIds, CancellationToken cancellationToken)
    {
        foreach (var fileId in fileIds) await fileStorage.DeleteAsync(fileId, cancellationToken);
    }

    private Task<bool> IsSessionActiveAsync(int userId, int sessionId, CancellationToken cancellationToken) =>
        repository.IsSessionActiveAsync(userId, sessionId, Now(), cancellationToken);

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;

    private static SecretChatResult Fail(string error) => new(null, error);
}
