using Domain.Entities;

namespace Application.Features.SecretChats;

/// <summary>Which device may act on a secret chat.</summary>
public static class SecretChatAccess
{
    public enum Side
    {
        None,
        Initiator,
        Participant
    }

    /// <summary>
    /// The initiator acts only from the device that started the chat. The participant acts from the device that
    /// accepted it; while the request is pending, any of their devices may see, accept or decline it.
    /// </summary>
    public static Side SideOf(SecretChat chat, int userId, int sessionId)
    {
        if (chat.InitiatorId == userId && chat.InitiatorSessionId == sessionId) return Side.Initiator;
        if (chat.ParticipantId != userId) return Side.None;
        if (chat.ParticipantSessionId == sessionId) return Side.Participant;
        return chat.ParticipantSessionId is null && chat.Status == SecretChatStatus.Pending ? Side.Participant : Side.None;
    }

    /// <summary>Only the two bound devices ever touch ciphertext.</summary>
    public static bool IsBound(SecretChat chat, int userId, int sessionId) =>
        (chat.InitiatorId == userId && chat.InitiatorSessionId == sessionId) ||
        (chat.ParticipantId == userId && chat.ParticipantSessionId == sessionId);
}
