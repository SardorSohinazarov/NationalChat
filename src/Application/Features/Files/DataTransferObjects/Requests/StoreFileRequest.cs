namespace Application.Features.Files.DataTransferObjects.Requests;

public sealed record StoreFileRequest(string FileName, string? DeclaredContentType, byte[] Content);
