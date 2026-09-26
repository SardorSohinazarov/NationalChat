using System.Security.Claims;
using API.DataTransferObjects.Responses;
using Application.Features.SecretChats;
using Application.Features.SecretChats.DataTransferObjects.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.SecretChats;

/// <summary>
/// End-to-end encrypted chats. Every endpoint acts as the current device (the access token's session), and the
/// server only relays public keys and ciphertext.
/// </summary>
[ApiController]
[Authorize]
[Route("api/secret-chats")]
public sealed class SecretChatController(ISecretChatService secretChatService, ISecretFileService secretFileService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(Result.Success(await secretChatService.ListAsync(GetCurrentUserId(), GetCurrentSessionId(), cancellationToken)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var chat = await secretChatService.GetAsync(GetCurrentUserId(), GetCurrentSessionId(), id, cancellationToken);
        return chat is null ? NotFound(Result.Fail("Maxfiy chat topilmadi.")) : Ok(Result.Success(chat));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSecretChatRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await secretChatService.CreateAsync(GetCurrentUserId(), GetCurrentSessionId(), request, cancellationToken));

    [HttpPost("{id:int}/accept")]
    public async Task<IActionResult> Accept(int id, AcceptSecretChatRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await secretChatService.AcceptAsync(GetCurrentUserId(), GetCurrentSessionId(), id, request, cancellationToken));

    /// <summary>Closes an active chat, cancels an outgoing request or declines an incoming one.</summary>
    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id, CancellationToken cancellationToken) =>
        ToActionResult(await secretChatService.CloseAsync(GetCurrentUserId(), GetCurrentSessionId(), id, cancellationToken));

    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> Send(int id, SendSecretMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await secretChatService.SendAsync(GetCurrentUserId(), GetCurrentSessionId(), id, request, cancellationToken);
        if (result.Message is not null) return Ok(Result.Success(result.Message));
        // 409 lets a client that retries after a lost response know the message is already queued.
        return result.Duplicate
            ? Conflict(Result.Fail(result.Error ?? "Xabar allaqachon qabul qilingan."))
            : BadRequest(Result.Fail(result.Error ?? "Xabarni yuborib bo'lmadi."));
    }

    /// <summary>Messages waiting for this device, oldest first. Acknowledge them so the server deletes them.</summary>
    [HttpGet("{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] SecretMessagesQuery query, CancellationToken cancellationToken)
    {
        var result = await secretChatService.GetMessagesAsync(GetCurrentUserId(), GetCurrentSessionId(), id, query, cancellationToken);
        return result.Page is null
            ? BadRequest(Result.Fail(result.Error ?? "Xabarlarni olib bo'lmadi."))
            : Ok(Result.Success(result.Page));
    }

    [HttpPost("{id:int}/ack")]
    public async Task<IActionResult> Ack(int id, AckSecretMessagesRequest request, CancellationToken cancellationToken)
    {
        var result = await secretChatService.AckAsync(GetCurrentUserId(), GetCurrentSessionId(), id, request, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(Result.Fail(result.Error ?? "Amalni bajarib bo'lmadi."));
    }

    /// <summary>
    /// Uploads an attachment already encrypted on the device (raw body, application/octet-stream). It is not
    /// scanned or probed: the server cannot read it. The key goes to the peer inside an encrypted message.
    /// </summary>
    [HttpPost("{id:int}/files")]
    [RequestSizeLimit(SecretChatLimits.MaxFileBytes + 64 * 1024)]
    public async Task<IActionResult> UploadFile(int id, CancellationToken cancellationToken)
    {
        var result = await secretFileService.UploadAsync(GetCurrentUserId(), GetCurrentSessionId(), id, Request.Body, Request.ContentLength, cancellationToken);
        return result.FileId is { } fileId
            ? Ok(Result.Success(new { fileId }))
            : BadRequest(Result.Fail(result.Error ?? "Faylni yuklab bo'lmadi."));
    }

    [HttpGet("{id:int}/files/{fileId:guid}")]
    public async Task<IActionResult> DownloadFile(int id, Guid fileId, CancellationToken cancellationToken)
    {
        var stream = await secretFileService.OpenAsync(GetCurrentUserId(), GetCurrentSessionId(), id, fileId, cancellationToken);
        return stream is null ? NotFound(Result.Fail("Fayl topilmadi.")) : File(stream, "application/octet-stream");
    }

    /// <summary>The recipient device has stored the file: the server copy is deleted.</summary>
    [HttpDelete("{id:int}/files/{fileId:guid}")]
    public async Task<IActionResult> DeleteFile(int id, Guid fileId, CancellationToken cancellationToken) =>
        await secretFileService.DeleteAsync(GetCurrentUserId(), GetCurrentSessionId(), id, fileId, cancellationToken)
            ? NoContent()
            : NotFound(Result.Fail("Fayl topilmadi."));

    private IActionResult ToActionResult(SecretChatResult result) =>
        result.Chat is null
            ? BadRequest(Result.Fail(result.Error ?? "Amalni bajarib bo'lmadi."))
            : Ok(Result.Success(result.Chat));

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private int GetCurrentSessionId() => int.Parse(User.FindFirstValue("sid")!);
}
