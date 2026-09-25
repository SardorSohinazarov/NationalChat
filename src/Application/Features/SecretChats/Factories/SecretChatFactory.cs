using Domain.Entities;

namespace Application.Features.SecretChats.Factories;

public static class SecretChatFactory
{
    public static SecretChat CreateRequest(int initiatorId, int initiatorSessionId, int participantId, string initiatorPublicKey, DateTime now) => new()
    {
        InitiatorId = initiatorId,
        InitiatorSessionId = initiatorSessionId,
        ParticipantId = participantId,
        InitiatorPublicKey = initiatorPublicKey,
        Status = SecretChatStatus.Pending,
        CreatedAt = now,
    };

    public static SecretMessage CreateMessage(int secretChatId, int senderSessionId, long seq, byte[] ciphertext, DateTime now) => new()
    {
        SecretChatId = secretChatId,
        SenderSessionId = senderSessionId,
        Seq = seq,
        Ciphertext = ciphertext,
        CreatedAt = now,
    };
}
