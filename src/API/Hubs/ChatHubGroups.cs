namespace API.Hubs;

public static class ChatHubGroups
{
    public static string User(int userId) => $"user:{userId}";

    public static string Chat(int chatId) => $"chat:{chatId}";

    /// <summary>One device (login session); secret chats are addressed to devices, not users.</summary>
    public static string Session(int sessionId) => $"session:{sessionId}";
}
