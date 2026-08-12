namespace Application.Features.Media.DataTransferObjects.Requests;

public sealed record UploadImageRequest(
    string FileName,
    string? DeclaredContentType,
    byte[] Content,
    string? TextContent,
    int? ReplyToMessageId);
