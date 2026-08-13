namespace Application.Features.Messages.DataTransferObjects.Requests;

public sealed record SendImageAttachmentRequest(string FileName, string? DeclaredContentType, byte[] Content, string? TextContent, int? ReplyToMessageId);
