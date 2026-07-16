using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.Users;
using Application.Features.Users.DataTransferObjects.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Users;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UserController(IUserDiscoveryService userDiscoveryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] UserSearchRequest request, CancellationToken cancellationToken) =>
        Ok(Result.Success(await userDiscoveryService.SearchAsync(GetCurrentUserId(), request, cancellationToken)));

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
