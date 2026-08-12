using System.Security.Claims;
using Application.Features.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize]
public sealed class ChatHub(
    IMessageRepository messageRepository,
    IChatRealtimeNotifier realtimeNotifier,
    TimeProvider timeProvider) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatHubGroups.User(GetCurrentUserId()));
        await base.OnConnectedAsync();
    }

    public async Task JoinChat(int chatId)
    {
        await EnsureChatMembershipAsync(chatId);
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatHubGroups.Chat(chatId));
    }

    public Task LeaveChat(int chatId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatHubGroups.Chat(chatId));

    public async Task SetTyping(int chatId, bool isTyping)
    {
        await EnsureChatMembershipAsync(chatId);
        await Clients.OthersInGroup(ChatHubGroups.Chat(chatId))
            .SendAsync("TypingChanged", new { chatId, userId = GetCurrentUserId(), isTyping });
    }

    public async Task MarkMessagesRead(int chatId, IReadOnlyCollection<int> messageIds)
    {
        await EnsureChatMembershipAsync(chatId);
        var ids = messageIds.Where(id => id > 0).Distinct().Take(100).ToArray();
        if (ids.Length == 0) return;

        var readerUserId = GetCurrentUserId();
        await messageRepository.MarkAsReadAsync(chatId, readerUserId, ids, timeProvider.GetUtcNow().UtcDateTime, Context.ConnectionAborted);
        await realtimeNotifier.MessagesReadAsync(chatId, readerUserId, ids, await messageRepository.GetMemberUserIdsAsync(chatId, Context.ConnectionAborted), Context.ConnectionAborted);
    }

    private async Task EnsureChatMembershipAsync(int chatId)
    {
        if (!await messageRepository.IsChatMemberAsync(chatId, GetCurrentUserId(), Context.ConnectionAborted))
        {
            throw new HubException("Bu chatga kirish huquqi yo'q.");
        }
    }

    private int GetCurrentUserId() =>
        int.Parse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException("Foydalanuvchi aniqlanmadi."));
}
