using System.Net.Mail;
using System.Security.Cryptography;
using Application.Features.Authentication.DataTransferObjects.Commands;
using Application.Features.Authentication.DataTransferObjects.Responses;
using Application.Features.Authentication.DataTransferObjects.Session;
using Domain.Entities;

namespace Application.Features.Authentication;

public sealed class AuthService(
    IAuthRepository store,
    IOneTimeCodeHasher codeHasher,
    IRefreshTokenHasher refreshTokenHasher,
    IAccessTokenIssuer accessTokenIssuer,
    IRegistrationTokenService registrationTokenService,
    IEmailSender emailSender,
    TimeProvider timeProvider,
    AuthOptions options) : IAuthService
{
    public async Task<SignInCodeRequestResult> RequestSignInCodeAsync(RequestSignInCodeCommand command, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(command.Email);
        if (email is null)
        {
            return new(false, null, "Email manzili noto'g'ri.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var latestCode = await store.GetLatestActiveVerificationCodeAsync(email, VerificationCodePurpose.SignIn, now, cancellationToken);
        if (latestCode is not null && now - latestCode.CreatedAt < options.ResendInterval)
        {
            return new(false, latestCode.CreatedAt.Add(options.ResendInterval), "Kod yaqinda yuborilgan.");
        }

        var requestedSince = now.Subtract(options.RequestWindow);
        if (await store.CountVerificationCodesAsync(email, requestedSince, cancellationToken) >= options.MaxRequestsPerWindow ||
            await store.CountVerificationCodesByIpAsync(command.IpAddress, requestedSince, cancellationToken) >= options.MaxRequestsPerWindow)
        {
            return new(false, requestedSince.Add(options.RequestWindow), "Kod yuborish limiti tugadi.");
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var expiresAt = now.Add(options.CodeLifetime);
        var verificationCode = new EmailVerificationCode
        {
            Email = email,
            CodeHash = codeHasher.Hash(code),
            Purpose = VerificationCodePurpose.SignIn,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            RequestIpAddress = command.IpAddress
        };
        await store.AddVerificationCodeAsync(verificationCode, cancellationToken);
        await store.SaveChangesAsync(cancellationToken);
        try
        {
            await emailSender.SendSignInCodeAsync(email, code, expiresAt, cancellationToken);
        }
        catch
        {
            verificationCode.ConsumedAt = now;
            await store.SaveChangesAsync(cancellationToken);
            throw;
        }

        return new(true, null, null);
    }

    public async Task<SignInVerificationResult> VerifySignInCodeAsync(VerifySignInCodeCommand command, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(command.Email);
        if (email is null || !IsSixDigitCode(command.Code))
        {
            return new(false, false, null, null, "Email yoki tasdiqlash kodi noto'g'ri.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var verification = await store.GetLatestActiveVerificationCodeAsync(email, VerificationCodePurpose.SignIn, now, cancellationToken);
        if (verification is null)
        {
            return new(false, false, null, null, "Kod topilmadi yoki muddati tugagan.");
        }

        if (!codeHasher.Verify(command.Code, verification.CodeHash))
        {
            verification.AttemptCount++;
            if (verification.AttemptCount >= options.MaxCodeAttempts)
            {
                verification.ConsumedAt = now;
            }

            await store.SaveChangesAsync(cancellationToken);
            return new(false, false, null, null, "Tasdiqlash kodi noto'g'ri.");
        }

        verification.ConsumedAt = now;
        var user = await store.FindUserByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsProfileCompleted)
        {
            await store.SaveChangesAsync(cancellationToken);
            var registrationToken = registrationTokenService.Create(email, now.Add(options.RegistrationTokenLifetime));
            return new(true, true, registrationToken, null, null);
        }

        var tokens = await CreateSessionAsync(user, command.Session, now, cancellationToken);
        return new(true, false, null, tokens, null);
    }

    public async Task<RegistrationResult> CompleteRegistrationAsync(CompleteRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var email = registrationTokenService.Validate(command.RegistrationToken, now);
        if (email is null)
        {
            return new(false, null, "Ro'yxatdan o'tish havolasi yaroqsiz yoki muddati tugagan.");
        }

        var username = NormalizeUsername(command.Username);
        if (username is null || string.IsNullOrWhiteSpace(command.FirstName))
        {
            return new(false, null, "Ism yoki username noto'g'ri.");
        }

        if (await store.FindUserByEmailAsync(email, cancellationToken) is not null || await store.UsernameExistsAsync(username, cancellationToken))
        {
            return new(false, null, "Email yoki username band.");
        }

        var user = new User
        {
            Email = email,
            Username = username,
            FirstName = command.FirstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(command.LastName) ? null : command.LastName.Trim(),
            IsProfileCompleted = true,
            CreatedAt = now
        };
        await store.AddUserAsync(user, cancellationToken);
        await store.SaveChangesAsync(cancellationToken);

        var tokens = await CreateSessionAsync(user, command.Session, now, cancellationToken);
        return new(true, tokens, null);
    }

    public async Task<TokenPair?> RefreshSessionAsync(RefreshSessionCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return null;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var session = await store.FindSessionByRefreshTokenHashAsync(refreshTokenHasher.Hash(command.RefreshToken), cancellationToken);
        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= now)
        {
            return null;
        }

        var refreshToken = CreateRefreshToken();
        session.RefreshTokenHash = refreshTokenHasher.Hash(refreshToken);
        session.LastActiveAt = now;
        session.IpAddress = command.IpAddress;
        session.ExpiresAt = now.Add(options.RefreshTokenLifetime);
        await store.SaveChangesAsync(cancellationToken);

        return new TokenPair(
            accessTokenIssuer.Create(session.UserId, session.Id, now.Add(options.AccessTokenLifetime)),
            now.Add(options.AccessTokenLifetime),
            refreshToken,
            session.ExpiresAt);
    }

    public async Task LogoutAsync(int userId, int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await store.FindSessionAsync(userId, sessionId, cancellationToken);
        if (session is null || session.RevokedAt is not null)
        {
            return;
        }

        session.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
        await store.SaveChangesAsync(cancellationToken);
    }

    public async Task LogoutAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var sessions = await store.GetActiveSessionsAsync(userId, now, cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        await store.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActiveSessionDto>> GetActiveSessionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var sessions = await store.GetActiveSessionsAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        return sessions.Select(x => new ActiveSessionDto(
            x.Id, x.DeviceName, x.SystemVersion, x.AppVersion, x.IpAddress,
            x.CreatedAt, x.LastActiveAt, x.ExpiresAt)).ToList();
    }

    private async Task<TokenPair> CreateSessionAsync(User user, AuthSessionMetadata metadata, DateTime now, CancellationToken cancellationToken)
    {
        var refreshToken = CreateRefreshToken();
        var expiresAt = now.Add(options.RefreshTokenLifetime);
        var session = new Session
        {
            UserId = user.Id,
            DeviceName = Limit(metadata.DeviceName, 100, "Unknown device"),
            SystemVersion = Limit(metadata.SystemVersion, 50, "Unknown"),
            AppVersion = Limit(metadata.AppVersion, 50, "Unknown"),
            IpAddress = Limit(metadata.IpAddress, 45, "Unknown"),
            UserAgent = string.IsNullOrWhiteSpace(metadata.UserAgent) ? null : metadata.UserAgent[..Math.Min(metadata.UserAgent.Length, 512)],
            RefreshTokenHash = refreshTokenHasher.Hash(refreshToken),
            CreatedAt = now,
            LastActiveAt = now,
            ExpiresAt = expiresAt
        };
        await store.AddSessionAsync(session, cancellationToken);
        await store.SaveChangesAsync(cancellationToken);

        var accessTokenExpiresAt = now.Add(options.AccessTokenLifetime);
        return new TokenPair(
            accessTokenIssuer.Create(user.Id, session.Id, accessTokenExpiresAt),
            accessTokenExpiresAt,
            refreshToken,
            expiresAt);
    }

    private static string? NormalizeEmail(string email)
    {
        try
        {
            var parsed = new MailAddress(email.Trim());
            return string.Equals(parsed.Address, email.Trim(), StringComparison.OrdinalIgnoreCase)
                ? parsed.Address.ToLowerInvariant()
                : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string? NormalizeUsername(string username)
    {
        var normalized = username.Trim().ToLowerInvariant();
        if (normalized.Length is < 5 or > 50 || !normalized.All(x => char.IsAsciiLetterOrDigit(x) || x == '_'))
        {
            return null;
        }

        return normalized;
    }

    private static bool IsSixDigitCode(string code) => code.Length == 6 && code.All(char.IsAsciiDigit);

    private static string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string Limit(string value, int maxLength, string fallback)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized[..Math.Min(normalized.Length, maxLength)];
    }
}
