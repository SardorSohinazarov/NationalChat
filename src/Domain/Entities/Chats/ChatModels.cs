namespace Domain.Entities;

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
