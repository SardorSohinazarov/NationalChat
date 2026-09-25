using Application.Features.SecretChats;
using Application.Features.SecretChats.DataTransferObjects.Responses;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public sealed class SignalRSecretChatRealtimeNotifier(
    IHubContext<ChatHub> hubContext,
    ILogger<SignalRSecretChatRealtimeNotifier> logger) : ISecretChatRealtimeNotifier
{
    public Task RequestedAsync(SecretChatDto participantView, int participantUserId, CancellationToken cancellationToken = default) =>
        PublishAsync("SecretChatRequested", participantView, [ChatHubGroups.User(participantUserId)], cancellationToken);

    public async Task AcceptedAsync(SecretChatDto initiatorView, int initiatorSessionId, SecretChatDto participantView, int participantUserId, CancellationToken cancellationToken = default)
    {
        await PublishAsync("SecretChatAccepted", initiatorView, [ChatHubGroups.Session(initiatorSessionId)], cancellationToken);
        await PublishAsync("SecretChatAccepted", participantView, [ChatHubGroups.User(participantUserId)], cancellationToken);
    }

    public Task MessageReceivedAsync(SecretMessageDto message, int recipientSessionId, CancellationToken cancellationToken = default) =>
        PublishAsync("SecretMessageReceived", message, [ChatHubGroups.Session(recipientSessionId)], cancellationToken);

    public Task ClosedAsync(int secretChatId, IReadOnlyCollection<int> sessionIds, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default) =>
        PublishAsync("SecretChatClosed", new { secretChatId },
            [.. sessionIds.Select(ChatHubGroups.Session), .. userIds.Select(ChatHubGroups.User)], cancellationToken);

    private async Task PublishAsync(string eventName, object payload, IReadOnlyList<string> groups, CancellationToken cancellationToken)
    {
        try
        {
            await hubContext.Clients.Groups(groups).SendAsync(eventName, payload, cancellationToken);
        }
        catch (Exception exception)
        {
            // Already committed: devices pick the state up from the REST endpoints on their next sync.
            logger.LogWarning(exception, "Could not publish {EventName}", eventName);
        }
    }
}
