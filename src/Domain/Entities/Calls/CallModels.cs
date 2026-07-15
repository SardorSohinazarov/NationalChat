namespace Domain.Entities;

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
