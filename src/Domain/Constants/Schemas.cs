namespace Domain.Constants;

public static class Schemas
{
    // Users, Contacts, BlockedUsers
    public const string Identity = "identity";

    // Chats, ChatMembers, Groups, Channels, ChannelSubscribers, SecretChats
    public const string Chat = "chat";

    // Messages, Attachments, Reactions, Polls, PollOptions, PollVotes, MessageViews
    public const string Messaging = "messaging";

    // Files, Photos, Stickers, StickerSets
    public const string Storage = "storage";

    // Calls, CallParticipants
    public const string Call = "call";

    // Stories, StoryViews
    public const string Story = "story";

    // Bots
    public const string Bot = "bot";

    // Folders, FolderChats, SavedMessages, Subscriptions
    public const string Personal = "personal";

    // Session, TwoFactorAuth
    public const string Security = "security";
}
