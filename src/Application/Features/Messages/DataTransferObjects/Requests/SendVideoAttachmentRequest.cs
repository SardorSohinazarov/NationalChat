namespace Application.Features.Messages.DataTransferObjects.Requests;

public sealed record SendVideoAttachmentRequest(string FileName, string? DeclaredContentType, byte[] Content, string? TextContent, int? ReplyToMessageId);
