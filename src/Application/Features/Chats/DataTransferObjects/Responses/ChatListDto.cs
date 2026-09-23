using Domain.Entities;

namespace Application.Features.Chats.DataTransferObjects.Responses;

public sealed record ChatLastMessageDto(
    int Id,
    string? TextContent,
    DateTime SentAt,
    int SenderId,
    string SenderUsername,
    string SenderFirstName,
    MessageServiceAction? ServiceAction);

public sealed record GroupChatSummaryDto(string Title, int? PhotoId, int MemberCount);

public sealed record ChatListDto(
    int Id,
    ChatType Type,
    DateTime CreatedAt,
    PrivateChatParticipantDto? Participant,
    GroupChatSummaryDto? Group,
    ChatLastMessageDto? LastMessage,
    int UnreadCount);
