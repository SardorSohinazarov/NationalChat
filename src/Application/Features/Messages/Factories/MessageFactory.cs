using Domain.Entities;

namespace Application.Features.Messages.Factories;

public static class MessageFactory
{
    public static Message Create(int chatId, int senderId, string textContent, int? replyToMessageId, DateTime sentAt) =>
        new()
        {
            ChatId = chatId,
            SenderId = senderId,
            TextContent = textContent.Trim(),
            ReplyToMessageId = replyToMessageId,
            SentAt = sentAt
        };

    /// <summary>
    /// Creates a service message (e.g. "X added Y"). <paramref name="textContent"/> keeps a snapshot
    /// of the names or title involved so the history stays readable after later renames.
    /// </summary>
    public static Message CreateService(int chatId, int actorId, MessageServiceAction action, string? textContent, DateTime sentAt) =>
        new()
        {
            ChatId = chatId,
            SenderId = actorId,
            ServiceAction = action,
            TextContent = textContent,
            SentAt = sentAt
        };
}
