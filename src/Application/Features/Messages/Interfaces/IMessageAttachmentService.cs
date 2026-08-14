using Application.DataTransferObjects.Pagination;
using Application.Features.Messages.DataTransferObjects.Requests;
using Application.Features.Messages.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Messages;

public interface IMessageAttachmentService
{
    Task<MessageAttachmentResult> SendImageAsync(int currentUserId, int chatId, SendImageAttachmentRequest request, CancellationToken cancellationToken = default);
    Task<MessageAttachmentResult> SendFileAsync(int currentUserId, int chatId, SendFileAttachmentRequest request, CancellationToken cancellationToken = default);
    Task<ProtectedAttachment?> GetImageAsync(int currentUserId, int fileId, bool original = false, CancellationToken cancellationToken = default);
    Task<ProtectedAttachment?> GetFileAsync(int currentUserId, int fileId, CancellationToken cancellationToken = default);
    Task<AttachmentSummaryDto?> GetAttachmentSummaryAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default);
    Task<CursorPagedResponse<MessageAttachmentDto>?> GetAttachmentsAsync(int currentUserId, int chatId, AttachmentType type, CursorPaginationRequest pagination, CancellationToken cancellationToken = default);
}

public sealed record MessageAttachmentResult(MessageDto? Message, string? Error);
public sealed record ProtectedAttachment(Stream Content, string MimeType, string FileName);
