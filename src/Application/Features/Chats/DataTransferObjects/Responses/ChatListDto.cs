using Domain.Entities;

namespace Application.Features.Chats.DataTransferObjects.Responses;

public sealed record ChatLastMessageDto(int Id, string? TextContent, DateTime SentAt, int SenderId, string SenderUsername);

public sealed record ChatListDto(
    int Id,
    ChatType Type,
    DateTime CreatedAt,
    PrivateChatParticipantDto? Participant,
    ChatLastMessageDto? LastMessage,
    int UnreadCount);
