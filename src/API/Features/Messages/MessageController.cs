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
    [HttpGet("search")]
    public async Task<IActionResult> Search(int chatId, [FromQuery] MessageSearchRequest request, CancellationToken cancellationToken)
    {
        var messages = await messageService.SearchAsync(GetCurrentUserId(), chatId, request, cancellationToken);
        return messages is null ? NotFound(Result.Fail("Chat topilmadi yoki unga kirish huquqi yo'q.")) : Ok(Result.Success(messages));
    }

    [HttpGet("{messageId:int}/context")]
    public async Task<IActionResult> GetContext(int chatId, int messageId, CancellationToken cancellationToken)
    {
        var messages = await messageService.GetContextAsync(GetCurrentUserId(), chatId, messageId, cancellationToken);
        return messages is null ? NotFound(Result.Fail("Xabar topilmadi yoki unga kirish huquqi yo'q.")) : Ok(Result.Success(messages));
    }
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
