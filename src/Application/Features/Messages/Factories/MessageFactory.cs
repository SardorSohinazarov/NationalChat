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
}
