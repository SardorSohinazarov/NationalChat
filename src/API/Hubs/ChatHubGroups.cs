namespace API.Hubs;

public static class ChatHubGroups
{
    public static string User(int userId) => $"user:{userId}";

    public static string Chat(int chatId) => $"chat:{chatId}";
}
