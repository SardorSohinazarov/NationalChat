using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.Chats;
using Application.Features.Chats.DataTransferObjects.Requests;
using Application.DataTransferObjects.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Chats;

[ApiController]
[Authorize]
[Route("api/chats")]
public sealed class ChatController(IChatService chatService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetChats([FromQuery] CursorPaginationRequest pagination, CancellationToken cancellationToken) =>
        Ok(Result.Success(await chatService.GetChatsAsync(GetCurrentUserId(), pagination, cancellationToken)));

    [HttpPost("private")]
    public async Task<IActionResult> FindOrCreatePrivateChat(CreatePrivateChatRequest request, CancellationToken cancellationToken)
    {
        var chat = await chatService.FindOrCreatePrivateChatAsync(GetCurrentUserId(), request, cancellationToken);
        return chat is null
            ? BadRequest(Result.Fail("Private chat yaratib bo'lmadi."))
            : Ok(Result.Success(chat));
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
