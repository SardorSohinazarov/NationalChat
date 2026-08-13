namespace Application.Features.Messages.DataTransferObjects.Responses;

public sealed record MessageAttachmentDto(
    int Id,
    int Type,
    string FileName,
    string MimeType,
    int SizeBytes,
    int Width,
    int Height,
    string ContentUrl);
