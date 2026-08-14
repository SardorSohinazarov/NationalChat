namespace Application.Features.Messages.DataTransferObjects.Requests;

public sealed record SendFileAttachmentRequest(string FileName, string? DeclaredContentType, byte[] Content, string? TextContent, int? ReplyToMessageId);
