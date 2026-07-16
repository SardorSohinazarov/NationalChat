using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.DataTransferObjects.Pagination;
using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Messages;

[ApiController]
[Authorize]
[Route("api/chats/{chatId:int}/messages")]
public sealed class MessageController(IMessageService messageService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMessages(int chatId, [FromQuery] CursorPaginationRequest pagination, CancellationToken cancellationToken)
    {
        var messages = await messageService.GetMessagesAsync(GetCurrentUserId(), chatId, pagination, cancellationToken);
        return messages is null
            ? NotFound(Result.Fail("Chat topilmadi yoki unga kirish huquqi yo'q."))
            : Ok(Result.Success(messages));
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(int chatId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await messageService.SendAsync(GetCurrentUserId(), chatId, request, cancellationToken);
        return message is null
            ? BadRequest(Result.Fail("Xabar yuborib bo'lmadi."))
            : Created($"api/chats/{chatId}/messages/{message.Id}", Result.Success(message));
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
