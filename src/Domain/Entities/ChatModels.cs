using File = Domain.Entities.File; 

namespace Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public int? ProfilePhotoId { get; set; }
    public bool IsProfileCompleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public Photo? ProfilePhoto { get; set; }
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<ChatMember> ChatMemberships { get; set; } = new List<ChatMember>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Group> CreatedGroups { get; set; } = new List<Group>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Contact> ContactedBy { get; set; } = new List<Contact>();
    public ICollection<Bot> Bots { get; set; } = new List<Bot>();
}

public class Chat
{
    public int Id { get; set; }
    public ChatType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
}

public class ChatMember
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int UserId { get; set; }
    public ChatMemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; }

    public Chat Chat { get; set; }
    public User User { get; set; }
}

public class Message
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string? TextContent { get; set; }
    public DateTime SentAt { get; set; }
    public int? ReplyToMessageId { get; set; }

    public Chat Chat { get; set; }
    public User Sender { get; set; }
    public Message? ReplyToMessage { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
    public ICollection<MessageView> Views { get; set; } = new List<MessageView>();
    public ICollection<SavedMessage> SavedMessages { get; set; } = new List<SavedMessage>();
}

public class Poll
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public bool IsClosed { get; set; }
    public PollType Type { get; set; }

    public Message Message { get; set; }
    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
}

public class PollOption
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public string TextVal { get; set; } = string.Empty;

    public Poll? Poll { get; set; }
    public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
}

public class PollVote
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public int OptionId { get; set; }
    public int UserId { get; set; }
    public DateTime VotedAt { get; set; }

    public Poll Poll { get; set; }
    public User User { get; set; }
    public PollOption Option { get; set; }
}

public class Call
{
    public int Id { get; set; }
    public int HostId { get; set; }
    public int ChatId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public CallType Type { get; set; }

    public User Host { get; set; }
    public Chat Chat { get; set; }
    public ICollection<CallParticipant> Participants { get; set; } = new List<CallParticipant>();
}

public class Photo
{
    public int Id { get; set; }
    public int FileId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public File File { get; set; }
}

public class Group
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? InviteLink { get; set; }
    public int CreatorId { get; set; }

    public Chat Chat { get; set; }
    public User Creator { get; set; }
}

public class Sticker
{
    public int Id { get; set; }
    public int SetId { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public int FileId { get; set; }

    public File File { get; set; }
    public StickerSet StickerSet { get; set; }
}

public class StickerSet
{
    public int Id { get; set; }
    public int CreatorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsAnimated { get; set; }

    public User Creator { get; set; }
    public ICollection<Sticker> Stickers { get; set; } = new List<Sticker>();
}

public class FolderChat
{
    public int Id { get; set; }
    public int FolderId { get; set; }
    public int ChatId { get; set; }

    public Folder Folder { get; set; }
    public Chat Chat { get; set; }
}

public class Folder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public User User { get; set; }
    public ICollection<FolderChat> FolderChats { get; set; } = new List<FolderChat>();
}

public class Attachment
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int FileId { get; set; }
    public AttachmentType Type { get; set; }

    public Message Message { get; set; }
    public File File { get; set; }
}

public class Story
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FileId { get; set; }
    public string? Caption { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int ViewCount { get; set; }

    public User User { get; set; }
    public File File { get; set; }
    public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
}

public class Subscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal PricePaid { get; set; }

    public User User { get; set; }
}

public class Session
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string SystemVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; }
}

public class EmailVerificationCode
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public VerificationCodePurpose Purpose { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string? RequestIpAddress { get; set; }
}

public class Reaction
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public DateTime ReactedAt { get; set; }

    public Message Message { get; set; }
    public User User { get; set; }
}

public class BlockedUser
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BlockedUserId { get; set; }
    public DateTime BlockedAt { get; set; }

    public User User { get; set; }
    public User BlockedUserRef { get; set; }
}

public class ChannelSubscriber
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; }

    public Channel Channel { get; set; }
    public User User { get; set; }
}

public class SavedMessage
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MessageId { get; set; }
    public DateTime SavedAt { get; set; }

    public User User { get; set; }
    public Message Message { get; set; }
}

public class Bot
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Commands { get; set; }
    public bool CanJoinGroups { get; set; }
    public bool InlineFeedback { get; set; }

    public User User { get; set; }
}

public class Contact
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ContactUserId { get; set; }
    public string? CustomFirstName { get; set; }
    public string? CustomLastName { get; set; }

    public User User { get; set; }
    public User ContactUser { get; set; }
}

public class SecretChat
{
    public int Id { get; set; }
    public int InitiatorId { get; set; }
    public int ParticipantId { get; set; }
    public string EncryptionKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public User Initiator { get; set; }
    public User Participant { get; set; }
}

public class Channel
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Description { get; set; }
    public string? InviteLink { get; set; }
    public bool SignatureEnabled { get; set; }

    public Chat Chat { get; set; }
    public ICollection<ChannelSubscriber> Subscribers { get; set; } = new List<ChannelSubscriber>();
}

public class CallParticipant
{
    public int Id { get; set; }
    public int CallId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }

    public Call Call { get; set; }
    public User User { get; set; }
}

public class StoryView
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public int UserId { get; set; }
    public DateTime ViewedAt { get; set; }

    public Story Story { get; set; }
    public User User { get; set; }
}

public class MessageView
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int UserId { get; set; }
    public DateTime ViewedAt { get; set; }

    public Message Message { get; set; }
    public User User { get; set; }
}

public class TwoFactorAuth
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public string? RecoveryEmail { get; set; }

    public User User { get; set; }
}

public class File
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
