using System.Security.Claims;
using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Profiles;
using Application.Features.Profiles.DataTransferObjects.Requests;
using API.DataTransferObjects.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Profiles;

[ApiController]
[Authorize]
[Route("api/users/me")]
public sealed class ProfileController(IProfileService profileService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var profile = await profileService.GetMyProfileAsync(GetUserId(), cancellationToken);
        return profile is null ? Unauthorized(Result.Fail("User aniqlanmadi")) : Ok(Result.Success(profile));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await profileService.UpdateMyProfileAsync(GetUserId(), request, cancellationToken);
        return profile is null
            ? BadRequest(Result.Fail("Profil ma'lumotlari noto'g'ri yoki username band."))
            : Ok(Result.Success(profile));
    }

    [HttpPost("photo")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> UploadMyPhoto(IFormFile? photo, CancellationToken cancellationToken)
    {
        if (photo is null || photo.Length == 0) return BadRequest(Result.Fail("Rasm tanlanmagan."));
        await using var input = photo.OpenReadStream();
        using var content = new MemoryStream();
        await input.CopyToAsync(content, cancellationToken);
        var result = await profileService.UpdateMyPhotoAsync(GetUserId(), new StoreImageRequest(photo.FileName, photo.ContentType, content.ToArray(), CropThumbnail: true), cancellationToken);
        return result.Profile is null ? BadRequest(Result.Fail(result.Error ?? "Rasm yuklanmadi.")) : Ok(Result.Success(result.Profile));
    }

    [HttpGet("~/api/media/profile-photos/{photoId:int}")]
    public async Task<IActionResult> GetProfilePhoto(int photoId, CancellationToken cancellationToken)
    {
        var photo = await profileService.GetPhotoAsync(photoId, original: false, cancellationToken);
        return photo is null ? NotFound(Result.Fail("Rasm topilmadi.")) : File(photo.Content, photo.MimeType, enableRangeProcessing: true);
    }

    [HttpGet("~/api/media/profile-photos/{photoId:int}/original")]
    public async Task<IActionResult> GetProfilePhotoOriginal(int photoId, CancellationToken cancellationToken)
    {
        var photo = await profileService.GetPhotoAsync(photoId, original: true, cancellationToken);
        return photo is null ? NotFound(Result.Fail("Rasm topilmadi.")) : File(photo.Content, photo.MimeType, enableRangeProcessing: true);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
