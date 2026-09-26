using Application.Features.SecretChats.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.SecretChats.Mappers;

public static class SecretChatMapper
{
    /// <summary>The chat as seen by <paramref name="viewerUserId"/> (initiator or participant).</summary>
    public static SecretChatDto ToDto(SecretChat chat, int viewerUserId)
    {
        var isInitiator = chat.InitiatorId == viewerUserId;
        return new SecretChatDto(
            chat.Id,
            chat.Status,
            isInitiator,
            ToPeer(isInitiator ? chat.Participant : chat.Initiator),
            isInitiator ? chat.InitiatorSessionId : chat.ParticipantSessionId,
            isInitiator ? chat.InitiatorPublicKey : chat.ParticipantPublicKey,
            isInitiator ? chat.ParticipantPublicKey : chat.InitiatorPublicKey,
            chat.CreatedAt,
            chat.AcceptedAt,
            chat.ClosedAt);
    }

    public static SecretMessageDto ToDto(SecretMessage message) =>
        new(message.Id, message.SecretChatId, message.Seq, Convert.ToBase64String(message.Ciphertext), message.CreatedAt);

    private static SecretChatPeerDto ToPeer(User user) =>
        new(user.Id, user.Username, user.FirstName, user.LastName, user.ProfilePhotoId);
}
