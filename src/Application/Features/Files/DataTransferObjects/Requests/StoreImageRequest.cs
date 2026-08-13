namespace Application.Features.Files.DataTransferObjects.Requests;

public sealed record StoreImageRequest(string FileName, string? DeclaredContentType, byte[] Content);
