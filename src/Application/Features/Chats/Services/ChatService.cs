using Application.Features.Chats.DataTransferObjects.Requests;
using Application.Features.Chats.DataTransferObjects.Responses;
using Application.Features.Chats.Factories;
using Application.Features.Chats.Mappers;
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
            return PrivateChatMapper.ToDto(chat, participant.Id);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        chat = PrivateChatFactory.Create(currentUserId, participant.Id, now);
        await repository.AddAsync(chat, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return PrivateChatMapper.ToDto(chat, participant);
    }
}
