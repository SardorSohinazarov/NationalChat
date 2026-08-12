using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.Media;
using Application.Features.Media.DataTransferObjects.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Media;

[ApiController]
[Authorize]
[Route("api")]
public sealed class MediaController(IImageAttachmentService imageAttachmentService) : ControllerBase
{
    [HttpPost("chats/{chatId:int}/attachments/images")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(int chatId, IFormFile? image, [FromForm] string? textContent, [FromForm] int? replyToMessageId, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0) return BadRequest(Result.Fail("Rasm tanlanmagan."));
        if (image.Length > 10 * 1024 * 1024) return BadRequest(Result.Fail("Rasm hajmi 10 MB dan oshmasligi kerak."));
        await using var input = image.OpenReadStream();
        using var content = new MemoryStream();
        await input.CopyToAsync(content, cancellationToken);
        var result = await imageAttachmentService.UploadAsync(GetCurrentUserId(), chatId,
            new UploadImageRequest(image.FileName, image.ContentType, content.ToArray(), textContent, replyToMessageId), cancellationToken);
        return result.Message is null ? BadRequest(Result.Fail(result.Error ?? "Rasm yuborilmadi.")) : Created($"api/chats/{chatId}/messages/{result.Message.Id}", Result.Success(result.Message));
    }

    [HttpGet("media/images/{fileId:int}")]
    public Task<IActionResult> GetImage(int fileId, CancellationToken cancellationToken) => GetContent(fileId, cancellationToken);

    private async Task<IActionResult> GetContent(int fileId, CancellationToken cancellationToken)
    {
        var image = await imageAttachmentService.GetImageAsync(GetCurrentUserId(), fileId, cancellationToken);
        return image is null ? NotFound(Result.Fail("Rasm topilmadi yoki unga kirish huquqi yo'q.")) : File(image.Content, image.MimeType, enableRangeProcessing: true);
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
