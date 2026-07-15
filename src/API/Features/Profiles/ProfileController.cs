using System.Security.Claims;
using Application.Features.Profiles;
using Application.Features.Profiles.DataTransferObjects.Requests;
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
        return profile is null ? Problem(title: "User aniqlanmadi", statusCode: StatusCodes.Status401Unauthorized) : Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await profileService.UpdateMyProfileAsync(GetUserId(), request, cancellationToken);
        return profile is null
            ? Problem(title: "Profil yangilanmadi", detail: "Profil ma'lumotlari noto'g'ri yoki username band.", statusCode: StatusCodes.Status400BadRequest)
            : Ok(profile);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
