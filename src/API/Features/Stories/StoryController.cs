using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.Stories;
using Application.Features.Stories.DataTransferObjects.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Stories;

[ApiController]
[Authorize]
[Route("api/stories")]
public sealed class StoryController(IStoryService storyService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFeed(CancellationToken cancellationToken)
    {
        var feed = await storyService.GetFeedAsync(GetCurrentUserId(), cancellationToken);
        return Ok(Result.Success(feed));
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserStories(int userId, CancellationToken cancellationToken)
    {
        var stories = await storyService.GetUserStoriesAsync(GetCurrentUserId(), userId, cancellationToken);
        return Ok(Result.Success(stories));
    }

    [HttpPost]
    [RequestSizeLimit(51 * 1024 * 1024)]
    public async Task<IActionResult> Create(IFormFile? media, [FromForm] string? caption, CancellationToken cancellationToken)
    {
        if (media is null || media.Length == 0) return BadRequest(Result.Fail("Fayl tanlanmagan."));
        await using var input = media.OpenReadStream();
        using var content = new MemoryStream();
        await input.CopyToAsync(content, cancellationToken);
        var result = await storyService.CreateAsync(GetCurrentUserId(), new CreateStoryRequest(media.FileName, media.ContentType, content.ToArray(), caption), cancellationToken);
        return result.Story is null
            ? BadRequest(Result.Fail(result.Error ?? "Hikoya yuklanmadi."))
            : Created($"api/stories/{result.Story.Id}", Result.Success(result.Story));
    }

    [HttpGet("{storyId:int}/media")]
    public async Task<IActionResult> GetMedia(int storyId, CancellationToken cancellationToken)
    {
        var media = await storyService.GetMediaAsync(storyId, cancellationToken);
        return media is null ? NotFound(Result.Fail("Hikoya topilmadi.")) : File(media.Content, media.MimeType, enableRangeProcessing: true);
    }

    [HttpGet("{storyId:int}/media/thumbnail")]
    public async Task<IActionResult> GetThumbnail(int storyId, CancellationToken cancellationToken)
    {
        var thumbnail = await storyService.GetThumbnailAsync(storyId, cancellationToken);
        return thumbnail is null ? NotFound(Result.Fail("Eskiz mavjud emas.")) : File(thumbnail.Content, thumbnail.MimeType);
    }

    [HttpPost("{storyId:int}/view")]
    public async Task<IActionResult> View(int storyId, CancellationToken cancellationToken)
    {
        var story = await storyService.ViewAsync(GetCurrentUserId(), storyId, cancellationToken);
        return story is null ? NotFound(Result.Fail("Hikoya topilmadi yoki muddati o'tgan.")) : Ok(Result.Success(story));
    }

    [HttpGet("{storyId:int}/viewers")]
    public async Task<IActionResult> GetViewers(int storyId, CancellationToken cancellationToken)
    {
        var viewers = await storyService.GetViewersAsync(GetCurrentUserId(), storyId, cancellationToken);
        return viewers is null ? NotFound(Result.Fail("Hikoya topilmadi yoki unga kirish huquqi yo'q.")) : Ok(Result.Success(viewers));
    }

    [HttpDelete("{storyId:int}")]
    public async Task<IActionResult> Delete(int storyId, CancellationToken cancellationToken) =>
        await storyService.DeleteAsync(GetCurrentUserId(), storyId, cancellationToken)
            ? NoContent()
            : BadRequest(Result.Fail("Hikoyani o'chirib bo'lmadi."));

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
