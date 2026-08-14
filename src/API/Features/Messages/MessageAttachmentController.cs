using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.DataTransferObjects.Pagination;
using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Requests;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Messages;

[ApiController]
[Authorize]
[Route("api")]
public sealed class MessageAttachmentController(IMessageAttachmentService messageAttachmentService) : ControllerBase
{
    [HttpPost("chats/{chatId:int}/attachments/images")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(int chatId, IFormFile? image, [FromForm] string? textContent, [FromForm] int? replyToMessageId, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0) return BadRequest(Result.Fail("Rasm tanlanmagan."));
        await using var input = image.OpenReadStream();
        using var content = new MemoryStream();
        await input.CopyToAsync(content, cancellationToken);
        var result = await messageAttachmentService.SendImageAsync(GetCurrentUserId(), chatId, new SendImageAttachmentRequest(image.FileName, image.ContentType, content.ToArray(), textContent, replyToMessageId), cancellationToken);
        return result.Message is null ? BadRequest(Result.Fail(result.Error ?? "Rasm yuborilmadi.")) : Created($"api/chats/{chatId}/messages/{result.Message.Id}", Result.Success(result.Message));
    }

    [HttpPost("chats/{chatId:int}/attachments/files")]
    [RequestSizeLimit(51 * 1024 * 1024)]
    public async Task<IActionResult> UploadFile(int chatId, IFormFile? file, [FromForm] string? textContent, [FromForm] int? replyToMessageId, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest(Result.Fail("Fayl tanlanmagan."));
        await using var input = file.OpenReadStream();
        using var content = new MemoryStream();
        await input.CopyToAsync(content, cancellationToken);
        var result = await messageAttachmentService.SendFileAsync(GetCurrentUserId(), chatId, new SendFileAttachmentRequest(file.FileName, file.ContentType, content.ToArray(), textContent, replyToMessageId), cancellationToken);
        return result.Message is null ? BadRequest(Result.Fail(result.Error ?? "Fayl yuborilmadi.")) : Created($"api/chats/{chatId}/messages/{result.Message.Id}", Result.Success(result.Message));
    }

    [HttpGet("media/files/{fileId:int}")]
    public async Task<IActionResult> GetFile(int fileId, CancellationToken cancellationToken)
    {
        var file = await messageAttachmentService.GetFileAsync(GetCurrentUserId(), fileId, cancellationToken);
        return file is null ? NotFound(Result.Fail("Fayl topilmadi yoki unga kirish huquqi yo'q.")) : File(file.Content, file.MimeType, file.FileName, enableRangeProcessing: true);
    }

    [HttpGet("media/images/{fileId:int}")]
    public async Task<IActionResult> GetImage(int fileId, CancellationToken cancellationToken)
    {
        var image = await messageAttachmentService.GetImageAsync(GetCurrentUserId(), fileId, original: false, cancellationToken);
        return image is null ? NotFound(Result.Fail("Rasm topilmadi yoki unga kirish huquqi yo'q.")) : File(image.Content, image.MimeType, enableRangeProcessing: true);
    }

    [HttpGet("media/images/{fileId:int}/original")]
    public async Task<IActionResult> GetImageOriginal(int fileId, CancellationToken cancellationToken)
    {
        var image = await messageAttachmentService.GetImageAsync(GetCurrentUserId(), fileId, original: true, cancellationToken);
        return image is null ? NotFound(Result.Fail("Rasm topilmadi yoki unga kirish huquqi yo'q.")) : File(image.Content, image.MimeType, enableRangeProcessing: true);
    }

    [HttpGet("chats/{chatId:int}/attachments/summary")]
    public async Task<IActionResult> GetAttachmentSummary(int chatId, CancellationToken cancellationToken)
    {
        var summary = await messageAttachmentService.GetAttachmentSummaryAsync(GetCurrentUserId(), chatId, cancellationToken);
        return summary is null ? NotFound(Result.Fail("Chat topilmadi yoki unga kirish huquqi yo'q.")) : Ok(Result.Success(summary));
    }

    [HttpGet("chats/{chatId:int}/attachments")]
    public async Task<IActionResult> GetAttachments(int chatId, [FromQuery] int type, [FromQuery] CursorPaginationRequest pagination, CancellationToken cancellationToken)
    {
        var attachments = await messageAttachmentService.GetAttachmentsAsync(GetCurrentUserId(), chatId, (AttachmentType)type, pagination, cancellationToken);
        return attachments is null ? NotFound(Result.Fail("Chat topilmadi yoki unga kirish huquqi yo'q.")) : Ok(Result.Success(attachments));
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
