using Application.Features.Chats.DataTransferObjects.Requests;
using Application.Features.Chats.DataTransferObjects.Responses;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Chats;

public sealed class ChatService(
    IChatRepository repository,
    IValidator<CreatePrivateChatRequest> createPrivateChatValidator,
    TimeProvider timeProvider) : IChatService
{
    public async Task<PrivateChatDto?> FindOrCreatePrivateChatAsync(
        int currentUserId,
        CreatePrivateChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await createPrivateChatValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid || request.UserId == currentUserId)
        {
            return null;
        }

        var participant = await repository.FindUserAsync(request.UserId, cancellationToken);
        if (participant is null || !participant.IsProfileCompleted)
        {
            return null;
        }

        var chat = await repository.FindPrivateChatAsync(currentUserId, participant.Id, cancellationToken);
        if (chat is not null)
        {
            return Map(chat, participant.Id);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        chat = new Chat
        {
            Type = ChatType.Private,
            CreatedAt = now,
            Members =
            [
                new ChatMember { UserId = currentUserId, Role = ChatMemberRole.Member, JoinedAt = now },
                new ChatMember { UserId = participant.Id, Role = ChatMemberRole.Member, JoinedAt = now }
            ]
        };
        await repository.AddAsync(chat, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return new(chat.Id, chat.CreatedAt, new(participant.Id, participant.Username, participant.FirstName, participant.LastName, participant.ProfilePhotoId));
    }

    private static PrivateChatDto Map(Chat chat, int participantId)
    {
        var participant = chat.Members.Single(x => x.UserId == participantId).User;
        return new(chat.Id, chat.CreatedAt, new(participant.Id, participant.Username, participant.FirstName, participant.LastName, participant.ProfilePhotoId));
    }
}
