namespace Domain.Constants;

public static class Tables
{
    // Identity
    public const string Users = "users";
    public const string Contacts = "contacts";
    public const string BlockedUsers = "blocked_users";

    // Chat
    public const string Chats = "chats";
    public const string ChatMembers = "chat_members";
    public const string Groups = "groups";
    public const string Channels = "channels";
    public const string ChannelSubscribers = "channel_subscribers";
    public const string SecretChats = "secret_chats";

    // Messaging
    public const string Messages = "messages";
    public const string Attachments = "attachments";
    public const string Reactions = "reactions";
    public const string Polls = "polls";
    public const string PollOptions = "poll_options";
    public const string PollVotes = "poll_votes";
    public const string MessageViews = "message_views";

    // Storage
    public const string Files = "files";
    public const string Photos = "photos";
    public const string Stickers = "stickers";
    public const string StickerSets = "sticker_sets";

    // Calls
    public const string Calls = "calls";
    public const string CallParticipants = "call_participants";

    // Stories
    public const string Stories = "stories";
    public const string StoryViews = "story_views";

    // Bots
    public const string Bots = "bots";

    // Personal
    public const string Folders = "folders";
    public const string FolderChats = "folder_chats";
    public const string SavedMessages = "saved_messages";
    public const string Subscriptions = "subscriptions";

    // Security
    public const string Sessions = "sessions";
    public const string TwoFactorAuth = "two_factor_auth";
    public const string EmailVerificationCodes = "email_verification_codes";
}