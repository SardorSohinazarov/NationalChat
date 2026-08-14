using Application.Features.Messages.DataTransferObjects.Requests;
using Application.Features.Messages.DataTransferObjects.Responses;

namespace Application.Features.Messages;

public interface IMessageAttachmentService
{
    Task<MessageAttachmentResult> SendImageAsync(int currentUserId, int chatId, SendImageAttachmentRequest request, CancellationToken cancellationToken = default);
    Task<ProtectedAttachment?> GetImageAsync(int currentUserId, int fileId, CancellationToken cancellationToken = default);
    Task<MessageAttachmentResult> SendFileAsync(int currentUserId, int chatId, SendFileAttachmentRequest request, CancellationToken cancellationToken = default);
    Task<ProtectedAttachment?> GetFileAsync(int currentUserId, int fileId, CancellationToken cancellationToken = default);
}

public sealed record MessageAttachmentResult(MessageDto? Message, string? Error);
public sealed record ProtectedAttachment(Stream Content, string MimeType, string FileName);
