using Application.Features.Media.DataTransferObjects.Responses;

namespace Application.Features.Messages.DataTransferObjects.Responses;

public sealed record MessageSenderDto(int Id, string Username, string FirstName, string? LastName, int? ProfilePhotoId);
public sealed record MessageReplyDto(int Id, string TextContent, MessageSenderDto Sender);

public sealed record MessageDto(
    int Id,
    int ChatId,
    string TextContent,
    DateTime SentAt,
    DateTime? EditedAt,
    int? ReplyToMessageId,
    MessageReplyDto? ReplyToMessage,
    MessageSenderDto Sender,
    bool IsRead,
    IReadOnlyList<MessageAttachmentDto> Attachments);
