namespace Domain.Entities;

public class Chat
{
    public int Id { get; set; }
    public ChatType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

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

public class Group
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Secret token of the group's invite link; null when there is no active link.</summary>
    public string? InviteLink { get; set; }
    public int CreatorId { get; set; }
    public int? PhotoId { get; set; }
    /// <summary>
    /// Set for an organization's own group ("tuit.uz"): its verified members join automatically. The group is
    /// private — others get in only when an admin adds them or through the invite link.
    /// </summary>
    public int? OrganizationId { get; set; }

    public Chat Chat { get; set; }
    public User Creator { get; set; }
    public Photo? Photo { get; set; }
    public Organization? Organization { get; set; }
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

public class ChannelSubscriber
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; }

    public Channel Channel { get; set; }
    public User User { get; set; }
}

/// <summary>
/// End-to-end encrypted 1:1 chat bound to exactly one device (session) on each side.
/// The server only ever sees public keys and ciphertext; message keys never leave the devices.
/// </summary>
public class SecretChat
{
    public int Id { get; set; }
    public int InitiatorId { get; set; }
    public int InitiatorSessionId { get; set; }
    public int ParticipantId { get; set; }
    /// <summary>Bound when the participant accepts on one of their devices.</summary>
    public int? ParticipantSessionId { get; set; }
    /// <summary>Base64 X25519 public key of the initiator's device (never a private or shared key).</summary>
    public string InitiatorPublicKey { get; set; } = string.Empty;
    public string? ParticipantPublicKey { get; set; }
    public SecretChatStatus Status { get; set; }
    /// <summary>Highest message sequence number accepted from each side; replays and reordering are rejected.</summary>
    public long InitiatorLastSeq { get; set; }
    public long ParticipantLastSeq { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public User Initiator { get; set; }
    public User Participant { get; set; }
    public Session InitiatorSession { get; set; }
    public Session? ParticipantSession { get; set; }
    public ICollection<SecretMessage> Messages { get; set; } = new List<SecretMessage>();
}

/// <summary>
/// An encrypted message waiting for delivery (store-and-forward). It is deleted as soon as the
/// recipient device acknowledges it, so the server keeps no history.
/// </summary>
public class SecretMessage
{
    public long Id { get; set; }
    public int SecretChatId { get; set; }
    public int SenderSessionId { get; set; }
    public long Seq { get; set; }
    public byte[] Ciphertext { get; set; } = [];
    public DateTime CreatedAt { get; set; }

    public SecretChat SecretChat { get; set; }
}
