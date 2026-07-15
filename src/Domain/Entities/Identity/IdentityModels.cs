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

public class TwoFactorAuth
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public string? RecoveryEmail { get; set; }

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

public class BlockedUser
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BlockedUserId { get; set; }
    public DateTime BlockedAt { get; set; }

    public User User { get; set; }
    public User BlockedUserRef { get; set; }
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
