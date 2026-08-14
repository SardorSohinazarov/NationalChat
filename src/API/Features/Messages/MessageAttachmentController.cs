using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Messages;

[ApiController]
[Authorize]
[Route("api")]
public sealed class MessageAttachmentController(IMessageAttachmentService messageAttachmentService, IValidator<SendFileAttachmentRequest> sendFileAttachmentRequestValidator) : ControllerBase
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

    [HttpGet("media/images/{fileId:int}")]
    public async Task<IActionResult> GetImage(int fileId, CancellationToken cancellationToken)
    {
        var image = await messageAttachmentService.GetImageAsync(GetCurrentUserId(), fileId, cancellationToken);
        return image is null ? NotFound(Result.Fail("Rasm topilmadi yoki unga kirish huquqi yo'q.")) : File(image.Content, image.MimeType, enableRangeProcessing: true);
    }

    [HttpPost("chats/{chatId:int}/attachments/files")]
    [RequestSizeLimit(21 * 1024 * 1024)]
    public async Task<IActionResult> UploadFile(int chatId, IFormFile? file, [FromForm] string? textContent, [FromForm] int? replyToMessageId, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest(Result.Fail("Fayl tanlanmagan."));
        await using var input = file.OpenReadStream();
        using var content = new MemoryStream();
        await input.CopyToAsync(content, cancellationToken);
        var request = new SendFileAttachmentRequest(file.FileName, file.ContentType, content.ToArray(), textContent, replyToMessageId);

        var validation = await sendFileAttachmentRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(Result.Fail(string.Join(" ", validation.Errors.Select(error => error.ErrorMessage))));

        var result = await messageAttachmentService.SendFileAsync(GetCurrentUserId(), chatId, request, cancellationToken);
        return result.Message is null ? BadRequest(Result.Fail(result.Error ?? "Fayl yuborilmadi.")) : Created($"api/chats/{chatId}/messages/{result.Message.Id}", Result.Success(result.Message));
    }

    [HttpGet("media/files/{fileId:int}")]
    public async Task<IActionResult> GetFile(int fileId, CancellationToken cancellationToken)
    {
        var file = await messageAttachmentService.GetFileAsync(GetCurrentUserId(), fileId, cancellationToken);
        return file is null ? NotFound(Result.Fail("Fayl topilmadi yoki unga kirish huquqi yo'q.")) : File(file.Content, file.MimeType, file.FileName, enableRangeProcessing: true);
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
