using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Organizations;

[ApiController]
[Authorize]
[Route("api/organizations")]
public sealed class OrganizationController(IOrganizationService organizationService) : ControllerBase
{
    /// <summary>The current user's verified organization, or no data when they are not a member of any.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken) =>
        Ok(Result.Success(await organizationService.GetMineAsync(GetUserId(), cancellationToken)));

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
