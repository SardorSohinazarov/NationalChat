namespace Application.Features.Messages.DataTransferObjects.Requests;

public sealed record SendMessageRequest(string TextContent, int? ReplyToMessageId);
