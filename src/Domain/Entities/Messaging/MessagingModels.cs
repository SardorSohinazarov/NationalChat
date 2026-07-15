using File = Domain.Entities.File;

namespace Domain.Entities;

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

public class Attachment
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int FileId { get; set; }
    public AttachmentType Type { get; set; }

    public Message Message { get; set; }
    public File File { get; set; }
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

public class MessageView
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int UserId { get; set; }
    public DateTime ViewedAt { get; set; }

    public Message Message { get; set; }
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
