namespace Application.Features.Stories.DataTransferObjects.Requests;

public sealed record CreateStoryRequest(string FileName, string? DeclaredContentType, byte[] Content, string? Caption);
