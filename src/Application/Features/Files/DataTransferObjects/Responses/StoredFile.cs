namespace Application.Features.Files.DataTransferObjects.Responses;

public sealed record StoredFile(string StoragePath, string FileName, string MimeType, int SizeBytes, int Width, int Height);
