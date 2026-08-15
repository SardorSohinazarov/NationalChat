using System.Security.Claims;
using Application.Features.Authentication;
using Application.Features.Authentication.DataTransferObjects.Commands;
using Application.Features.Authentication.DataTransferObjects.Requests;
using Application.Features.Authentication.DataTransferObjects.Responses;
using Application.Features.Authentication.DataTransferObjects.Session;
using API.DataTransferObjects.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Authentication;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IWebHostEnvironment environment) : ControllerBase
{
    private const string RefreshCookieName = "nationalchat_refresh";

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode(RequestCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RequestSignInCodeAsync(
            new RequestSignInCodeCommand(request.Email, GetIpAddress()), cancellationToken);

        if (result.IsAccepted)
        {
            return Accepted(Result.Success());
        }

        return StatusCode(
            result.RetryAfter is null ? StatusCodes.Status400BadRequest : StatusCodes.Status429TooManyRequests,
            Result.Fail(result.Error));
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode(VerifyCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.VerifySignInCodeAsync(
            new VerifySignInCodeCommand(request.Email, request.Code, GetSessionMetadata(request.DeviceName, request.SystemVersion, request.AppVersion)),
            cancellationToken);

        if (!result.IsSuccessful)
        {
            return BadRequest(Result.Fail(result.Error));
        }

        if (result.RegistrationRequired)
        {
            return Ok(Result.Success(new { registrationRequired = true, registrationToken = result.RegistrationToken }));
        }

        WriteRefreshCookie(result.Tokens!);
        return Ok(Result.Success(new
        {
            accessToken = result.Tokens!.AccessToken,
            accessTokenExpiresAt = result.Tokens.AccessTokenExpiresAt
        }));
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
            return BadRequest(Result.Fail(result.Error));
        }

        WriteRefreshCookie(result.Tokens!);
        return Created("api/auth/sessions", Result.Success(new
        {
            accessToken = result.Tokens!.AccessToken,
            accessTokenExpiresAt = result.Tokens.AccessTokenExpiresAt
        }));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken))
        {
            return Unauthorized(Result.Fail("Refresh token topilmadi"));
        }

        var tokens = await authService.RefreshSessionAsync(new RefreshSessionCommand(refreshToken, GetIpAddress()), cancellationToken);
        if (tokens is null)
        {
            DeleteRefreshCookie();
            return Unauthorized(Result.Fail("Session yaroqsiz"));
        }

        WriteRefreshCookie(tokens);
        return Ok(Result.Success(new { accessToken = tokens.AccessToken, accessTokenExpiresAt = tokens.AccessTokenExpiresAt }));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentSession(out var userId, out var sessionId))
        {
            return Unauthorized(Result.Fail("Session yaroqsiz"));
        }

        await authService.LogoutAsync(userId, sessionId, cancellationToken);
        DeleteRefreshCookie();
        return Ok(Result.Success());
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(Result.Fail("User aniqlanmadi"));
        }

        await authService.LogoutAllAsync(userId.Value, cancellationToken);
        DeleteRefreshCookie();
        return Ok(Result.Success());
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentSession(out var userId, out var sessionId))
        {
            return Unauthorized(Result.Fail("Session yaroqsiz"));
        }

        return Ok(Result.Success(await authService.GetActiveSessionsAsync(userId, sessionId, cancellationToken)));
    }

    [Authorize]
    [HttpDelete("sessions/{id:int}")]
    public async Task<IActionResult> RevokeSession(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentSession(out var userId, out var sessionId))
        {
            return Unauthorized(Result.Fail("Session yaroqsiz"));
        }

        var revoked = await authService.RevokeSessionAsync(userId, id, sessionId, cancellationToken);
        if (!revoked)
        {
            return NotFound(Result.Fail("Sessiya topilmadi"));
        }

        return Ok(Result.Success());
    }

    [Authorize]
    [HttpPost("sessions/logout-others")]
    public async Task<IActionResult> LogoutOthers(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentSession(out var userId, out var sessionId))
        {
            return Unauthorized(Result.Fail("Session yaroqsiz"));
        }

        await authService.LogoutAllOthersAsync(userId, sessionId, cancellationToken);
        return Ok(Result.Success());
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
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(tokens.RefreshTokenExpiresAt),
            Path = "/api/auth"
        });
    }

    private void DeleteRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth", Secure = !environment.IsDevelopment(), SameSite = SameSiteMode.Strict });
}
