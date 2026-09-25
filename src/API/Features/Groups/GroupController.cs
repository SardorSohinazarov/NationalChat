using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Groups;
using Application.Features.Groups.DataTransferObjects.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Groups;

[ApiController]
[Authorize]
[Route("api/groups")]
public sealed class GroupController(IGroupService groupService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateGroupRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.CreateAsync(GetCurrentUserId(), request, cancellationToken));

    [HttpGet("{chatId:int}")]
    public async Task<IActionResult> Get(int chatId, CancellationToken cancellationToken)
    {
        var group = await groupService.GetAsync(GetCurrentUserId(), chatId, cancellationToken);
        return group is null ? NotFound(Result.Fail("Guruh topilmadi.")) : Ok(Result.Success(group));
    }

    [HttpPut("{chatId:int}")]
    public async Task<IActionResult> Update(int chatId, UpdateGroupRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.UpdateAsync(GetCurrentUserId(), chatId, request, cancellationToken));

    [HttpPost("{chatId:int}/photo")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(int chatId, IFormFile? photo, CancellationToken cancellationToken)
    {
        if (photo is null || photo.Length == 0) return BadRequest(Result.Fail("Rasm tanlanmagan."));
        await using var input = photo.OpenReadStream();
        using var content = new MemoryStream();
        await input.CopyToAsync(content, cancellationToken);
        var request = new StoreImageRequest(photo.FileName, photo.ContentType, content.ToArray(), CropThumbnail: true);
        return ToActionResult(await groupService.UpdatePhotoAsync(GetCurrentUserId(), chatId, request, cancellationToken));
    }

    [HttpPost("{chatId:int}/members")]
    public async Task<IActionResult> AddMembers(int chatId, AddGroupMembersRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.AddMembersAsync(GetCurrentUserId(), chatId, request, cancellationToken));

    [HttpDelete("{chatId:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(int chatId, int userId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.RemoveMemberAsync(GetCurrentUserId(), chatId, userId, cancellationToken));

    [HttpPut("{chatId:int}/members/{userId:int}/role")]
    public async Task<IActionResult> UpdateMemberRole(int chatId, int userId, UpdateGroupMemberRoleRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.UpdateMemberRoleAsync(GetCurrentUserId(), chatId, userId, request, cancellationToken));

    [HttpPost("{chatId:int}/leave")]
    public async Task<IActionResult> Leave(int chatId, CancellationToken cancellationToken)
    {
        var result = await groupService.LeaveAsync(GetCurrentUserId(), chatId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(Result.Fail(result.Error ?? "Guruhdan chiqib bo'lmadi."));
    }

    /// <summary>Creates a new invite link (the previous one stops working). Admins and the owner only.</summary>
    [HttpPost("{chatId:int}/invite-link")]
    public async Task<IActionResult> CreateInviteLink(int chatId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.CreateInviteLinkAsync(GetCurrentUserId(), chatId, cancellationToken));

    [HttpDelete("{chatId:int}/invite-link")]
    public async Task<IActionResult> RevokeInviteLink(int chatId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.RevokeInviteLinkAsync(GetCurrentUserId(), chatId, cancellationToken));

    [HttpGet("invites/{token}")]
    public async Task<IActionResult> GetInvite(string token, CancellationToken cancellationToken)
    {
        var preview = await groupService.GetInvitePreviewAsync(GetCurrentUserId(), token, cancellationToken);
        return preview is null
            ? NotFound(Result.Fail("Taklif havolasi yaroqsiz yoki bekor qilingan."))
            : Ok(Result.Success(preview));
    }

    [HttpPost("invites/{token}/join")]
    public async Task<IActionResult> JoinByInvite(string token, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.JoinByInviteAsync(GetCurrentUserId(), token, cancellationToken));

    private IActionResult ToActionResult(GroupResult result) =>
        result.Group is null
            ? BadRequest(Result.Fail(result.Error ?? "Amalni bajarib bo'lmadi."))
            : Ok(Result.Success(result.Group));

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
