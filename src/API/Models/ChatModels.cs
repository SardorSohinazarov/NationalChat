namespace API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public int? ProfilePhotoId { get; set; }

        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<ChatMember> ChatMemberships { get; set; } = new List<ChatMember>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }

    public class Chat
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    }

    public class ChatMember
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
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
    }

    public class Poll
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public Message Message { get; set; }
        public string Question { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public bool IsClosed { get; set; }
        public string Type { get; set; } = string.Empty;

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

        public PollOption? Option { get; set; }
    }

    public class Call
    {
        public int Id { get; set; }
        public int HostId { get; set; }
        public int ChatId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Type { get; set; } = string.Empty;

        public ICollection<CallParticipant> Participants { get; set; } = new List<CallParticipant>();
    }

    public class Photo
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? InviteLink { get; set; }
        public int CreatorId { get; set; }
        public User User { get; set; }
    }

    public class Sticker
    {
        public int Id { get; set; }
        public int SetId { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public int FileId { get; set; }
    }

    public class StickerSet
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsAnimated { get; set; }
    }

    public class FolderChat
    {
        public int Id { get; set; }
        public int FolderId { get; set; }
        public int ChatId { get; set; }
    }

    public class Folder
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class Attachment
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int FileId { get; set; }
        public string Type { get; set; } = string.Empty;

        public Message? Message { get; set; }
        public File? File { get; set; }
    }

    public class Story
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FileId { get; set; }
        public string? Caption { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ViewCount { get; set; }

        public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
    }

    public class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal PricePaid { get; set; }
    }

    public class Session
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string SystemVersion { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class Reaction
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public DateTime ReactedAt { get; set; }

        public Message? Message { get; set; }
        public User? User { get; set; }
    }

    public class BlockedUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BlockedUserId { get; set; }
        public DateTime BlockedAt { get; set; }
    }

    public class ChannelSubscriber
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class SavedMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MessageId { get; set; }
        public DateTime SavedAt { get; set; }
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
    }

    public class Contact
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ContactUserId { get; set; }
        public User ContactUser { get; set; }
        public string? CustomFirstName { get; set; }
        public string? CustomLastName { get; set; }
    }

    public class SecretChat
    {
        public int Id { get; set; }
        public int InitiatorId { get; set; }
        public int ParticipantId { get; set; }
        public string EncryptionKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
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
    }

    public class CallParticipant
    {
        public int Id { get; set; }
        public int CallId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
    }

    public class StoryView
    {
        public int Id { get; set; }
        public int StoryId { get; set; }
        public int UserId { get; set; }
        public DateTime ViewedAt { get; set; }
    }

    public class MessageView
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public DateTime ViewedAt { get; set; }
    }

    public class TwoFactorAuth
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string? Hint { get; set; }
        public string? RecoveryEmail { get; set; }
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
}