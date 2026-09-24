using Domain.Entities;
using Domain.Text;

namespace Application.Features.Messages.Factories;

public static class MessageFactory
{
    public static Message Create(int chatId, int senderId, string textContent, int? replyToMessageId, DateTime sentAt)
    {
        var text = textContent.Trim();
        return new()
        {
            ChatId = chatId,
            SenderId = senderId,
            TextContent = text,
            SearchText = BuildSearchText(text),
            ReplyToMessageId = replyToMessageId,
            SentAt = sentAt
        };
    }

    /// <summary>Script-independent search key, so "salom" also finds "салом". Null when there is no text.</summary>
    public static string? BuildSearchText(string? textContent)
    {
        var searchText = UzbekTransliterator.NormalizeForSearch(textContent);
        return searchText.Length == 0 ? null : searchText;
    }

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
