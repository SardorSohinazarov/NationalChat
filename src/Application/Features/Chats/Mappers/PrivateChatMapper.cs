using Application.Features.Chats.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Chats.Mappers;

public static class PrivateChatMapper
{
    public static PrivateChatDto ToDto(Chat chat, int participantId)
    {
        var participant = chat.Members.Single(x => x.UserId == participantId).User;
        return ToDto(chat, participant);
    }

    public static PrivateChatDto ToDto(Chat chat, User participant) =>
        new(chat.Id, chat.CreatedAt, new(participant.Id, participant.Username, participant.FirstName, participant.LastName, participant.ProfilePhotoId));
}
