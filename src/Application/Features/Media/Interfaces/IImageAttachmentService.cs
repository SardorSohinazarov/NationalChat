using Application.Features.Media.DataTransferObjects.Requests;
using Application.Features.Messages.DataTransferObjects.Responses;

namespace Application.Features.Media;

public interface IImageAttachmentService
{
    Task<ImageUploadResult> UploadAsync(int currentUserId, int chatId, UploadImageRequest request, CancellationToken cancellationToken = default);
    Task<ProtectedImage?> GetImageAsync(int currentUserId, int fileId, CancellationToken cancellationToken = default);
}

public sealed record ImageUploadResult(MessageDto? Message, string? Error);
public sealed record ProtectedImage(Stream Content, string MimeType, string FileName);
