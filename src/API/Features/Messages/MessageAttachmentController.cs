using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Requests;
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

    [HttpGet("media/images/{fileId:int}")]
    public async Task<IActionResult> GetImage(int fileId, CancellationToken cancellationToken)
    {
        var image = await messageAttachmentService.GetImageAsync(GetCurrentUserId(), fileId, cancellationToken);
        return image is null ? NotFound(Result.Fail("Rasm topilmadi yoki unga kirish huquqi yo'q.")) : File(image.Content, image.MimeType, enableRangeProcessing: true);
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
