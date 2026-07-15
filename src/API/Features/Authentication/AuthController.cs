using System.Security.Claims;
using Application.Features.Authentication;
using Application.Features.Authentication.DataTransferObjects.Commands;
using Application.Features.Authentication.DataTransferObjects.Requests;
using Application.Features.Authentication.DataTransferObjects.Responses;
using Application.Features.Authentication.DataTransferObjects.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Authentication;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    private const string RefreshCookieName = "nationalchat_refresh";

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode(RequestCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RequestSignInCodeAsync(
            new RequestSignInCodeCommand(request.Email, GetIpAddress()), cancellationToken);

        if (result.IsAccepted)
        {
            return Accepted();
        }

        return result.RetryAfter is null
            ? Problem(title: "Tasdiqlash kodi yuborilmadi", detail: result.Error, statusCode: StatusCodes.Status400BadRequest)
            : Problem(title: "So'rov limiti tugadi", detail: result.Error, statusCode: StatusCodes.Status429TooManyRequests);
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode(VerifyCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.VerifySignInCodeAsync(
            new VerifySignInCodeCommand(request.Email, request.Code, GetSessionMetadata(request.DeviceName, request.SystemVersion, request.AppVersion)),
            cancellationToken);

        if (!result.IsSuccessful)
        {
            return Problem(title: "Tasdiqlash kodi noto'g'ri", detail: result.Error, statusCode: StatusCodes.Status400BadRequest);
        }

        if (result.RegistrationRequired)
        {
            return Ok(new { registrationRequired = true, registrationToken = result.RegistrationToken });
        }

        WriteRefreshCookie(result.Tokens!);
        return Ok(new
        {
            accessToken = result.Tokens!.AccessToken,
            accessTokenExpiresAt = result.Tokens.AccessTokenExpiresAt
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.CompleteRegistrationAsync(
            new CompleteRegistrationCommand(
                request.RegistrationToken,
                request.Username,
                request.FirstName,
                request.LastName,
                GetSessionMetadata(request.DeviceName, request.SystemVersion, request.AppVersion)),
            cancellationToken);

        if (!result.IsSuccessful)
        {
            return Problem(title: "Ro'yxatdan o'tish bajarilmadi", detail: result.Error, statusCode: StatusCodes.Status400BadRequest);
        }

        WriteRefreshCookie(result.Tokens!);
        return Created("api/auth/sessions", new
        {
            accessToken = result.Tokens!.AccessToken,
            accessTokenExpiresAt = result.Tokens.AccessTokenExpiresAt
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken))
        {
            return Problem(title: "Refresh token topilmadi", statusCode: StatusCodes.Status401Unauthorized);
        }

        var tokens = await authService.RefreshSessionAsync(new RefreshSessionCommand(refreshToken, GetIpAddress()), cancellationToken);
        if (tokens is null)
        {
            DeleteRefreshCookie();
            return Problem(title: "Session yaroqsiz", statusCode: StatusCodes.Status401Unauthorized);
        }

        WriteRefreshCookie(tokens);
        return Ok(new { accessToken = tokens.AccessToken, accessTokenExpiresAt = tokens.AccessTokenExpiresAt });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentSession(out var userId, out var sessionId))
        {
            return Problem(title: "Session yaroqsiz", statusCode: StatusCodes.Status401Unauthorized);
        }

        await authService.LogoutAsync(userId, sessionId, cancellationToken);
        DeleteRefreshCookie();
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Problem(title: "User aniqlanmadi", statusCode: StatusCodes.Status401Unauthorized);
        }

        await authService.LogoutAllAsync(userId.Value, cancellationToken);
        DeleteRefreshCookie();
        return NoContent();
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Problem(title: "User aniqlanmadi", statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(await authService.GetActiveSessionsAsync(userId.Value, cancellationToken));
    }

    private AuthSessionMetadata GetSessionMetadata(string? deviceName, string? systemVersion, string? appVersion) =>
        new(
            deviceName ?? "Web browser",
            systemVersion ?? "Unknown",
            appVersion ?? "Web",
            GetIpAddress(),
            Request.Headers.UserAgent.ToString());

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

    private int? GetCurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private bool TryGetCurrentSession(out int userId, out int sessionId)
    {
        userId = GetCurrentUserId() ?? 0;
        sessionId = 0;
        return userId > 0 && int.TryParse(User.FindFirstValue("sid"), out sessionId);
    }

    private void WriteRefreshCookie(TokenPair tokens)
    {
        Response.Cookies.Append(RefreshCookieName, tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(tokens.RefreshTokenExpiresAt),
            Path = "/api/auth"
        });
    }

    private void DeleteRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth", Secure = true, SameSite = SameSiteMode.Strict });
}
