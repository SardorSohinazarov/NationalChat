namespace Application.Features.Messages.DataTransferObjects.Responses;

public sealed record MessageSenderDto(int Id, string Username, string FirstName, string? LastName, int? ProfilePhotoId);

public sealed record MessageDto(
    int Id,
    int ChatId,
    string TextContent,
    DateTime SentAt,
    int? ReplyToMessageId,
    MessageSenderDto Sender,
    bool IsRead);
