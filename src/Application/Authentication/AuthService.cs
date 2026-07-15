using System.Net.Mail;
using System.Security.Cryptography;
using Domain.Entities;

namespace Application.Authentication;

public sealed class AuthService : IAuthService
{
    private readonly IAuthStore _store;
    private readonly IOneTimeCodeHasher _codeHasher;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IRegistrationTokenService _registrationTokenService;
    private readonly IEmailSender _emailSender;
    private readonly TimeProvider _timeProvider;
    private readonly AuthOptions _options;

    public AuthService(
        IAuthStore store,
        IOneTimeCodeHasher codeHasher,
        IRefreshTokenHasher refreshTokenHasher,
        IAccessTokenIssuer accessTokenIssuer,
        IRegistrationTokenService registrationTokenService,
        IEmailSender emailSender,
        TimeProvider timeProvider,
        AuthOptions options)
    {
        _store = store;
        _codeHasher = codeHasher;
        _refreshTokenHasher = refreshTokenHasher;
        _accessTokenIssuer = accessTokenIssuer;
        _registrationTokenService = registrationTokenService;
        _emailSender = emailSender;
        _timeProvider = timeProvider;
        _options = options;
    }

    public async Task<SignInCodeRequestResult> RequestSignInCodeAsync(RequestSignInCodeCommand command, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(command.Email);
        if (email is null)
        {
            return new(false, null, "Email manzili noto'g'ri.");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var latestCode = await _store.GetLatestActiveVerificationCodeAsync(email, VerificationCodePurpose.SignIn, now, cancellationToken);
        if (latestCode is not null && now - latestCode.CreatedAt < _options.ResendInterval)
        {
            return new(false, latestCode.CreatedAt.Add(_options.ResendInterval), "Kod yaqinda yuborilgan.");
        }

        var requestedSince = now.Subtract(_options.RequestWindow);
        if (await _store.CountVerificationCodesAsync(email, requestedSince, cancellationToken) >= _options.MaxRequestsPerWindow ||
            await _store.CountVerificationCodesByIpAsync(command.IpAddress, requestedSince, cancellationToken) >= _options.MaxRequestsPerWindow)
        {
            return new(false, requestedSince.Add(_options.RequestWindow), "Kod yuborish limiti tugadi.");
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var expiresAt = now.Add(_options.CodeLifetime);
        var verificationCode = new EmailVerificationCode
        {
            Email = email,
            CodeHash = _codeHasher.Hash(code),
            Purpose = VerificationCodePurpose.SignIn,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            RequestIpAddress = command.IpAddress
        };
        await _store.AddVerificationCodeAsync(verificationCode, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);
        try
        {
            await _emailSender.SendSignInCodeAsync(email, code, expiresAt, cancellationToken);
        }
        catch
        {
            verificationCode.ConsumedAt = now;
            await _store.SaveChangesAsync(cancellationToken);
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

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var verification = await _store.GetLatestActiveVerificationCodeAsync(email, VerificationCodePurpose.SignIn, now, cancellationToken);
        if (verification is null)
        {
            return new(false, false, null, null, "Kod topilmadi yoki muddati tugagan.");
        }

        if (!_codeHasher.Verify(command.Code, verification.CodeHash))
        {
            verification.AttemptCount++;
            if (verification.AttemptCount >= _options.MaxCodeAttempts)
            {
                verification.ConsumedAt = now;
            }

            await _store.SaveChangesAsync(cancellationToken);
            return new(false, false, null, null, "Tasdiqlash kodi noto'g'ri.");
        }

        verification.ConsumedAt = now;
        var user = await _store.FindUserByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsProfileCompleted)
        {
            await _store.SaveChangesAsync(cancellationToken);
            var registrationToken = _registrationTokenService.Create(email, now.Add(_options.RegistrationTokenLifetime));
            return new(true, true, registrationToken, null, null);
        }

        var tokens = await CreateSessionAsync(user, command.Session, now, cancellationToken);
        return new(true, false, null, tokens, null);
    }

    public async Task<RegistrationResult> CompleteRegistrationAsync(CompleteRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var email = _registrationTokenService.Validate(command.RegistrationToken, now);
        if (email is null)
        {
            return new(false, null, "Ro'yxatdan o'tish havolasi yaroqsiz yoki muddati tugagan.");
        }

        var username = NormalizeUsername(command.Username);
        if (username is null || string.IsNullOrWhiteSpace(command.FirstName))
        {
            return new(false, null, "Ism yoki username noto'g'ri.");
        }

        if (await _store.FindUserByEmailAsync(email, cancellationToken) is not null || await _store.UsernameExistsAsync(username, cancellationToken))
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
        await _store.AddUserAsync(user, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);

        var tokens = await CreateSessionAsync(user, command.Session, now, cancellationToken);
        return new(true, tokens, null);
    }

    public async Task<TokenPair?> RefreshSessionAsync(RefreshSessionCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return null;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var session = await _store.FindSessionByRefreshTokenHashAsync(_refreshTokenHasher.Hash(command.RefreshToken), cancellationToken);
        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= now)
        {
            return null;
        }

        var refreshToken = CreateRefreshToken();
        session.RefreshTokenHash = _refreshTokenHasher.Hash(refreshToken);
        session.LastActiveAt = now;
        session.IpAddress = command.IpAddress;
        session.ExpiresAt = now.Add(_options.RefreshTokenLifetime);
        await _store.SaveChangesAsync(cancellationToken);

        return new TokenPair(
            _accessTokenIssuer.Create(session.UserId, session.Id, now.Add(_options.AccessTokenLifetime)),
            now.Add(_options.AccessTokenLifetime),
            refreshToken,
            session.ExpiresAt);
    }

    public async Task LogoutAsync(int userId, int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _store.FindSessionAsync(userId, sessionId, cancellationToken);
        if (session is null || session.RevokedAt is not null)
        {
            return;
        }

        session.RevokedAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _store.SaveChangesAsync(cancellationToken);
    }

    public async Task LogoutAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var sessions = await _store.GetActiveSessionsAsync(userId, now, cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        await _store.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActiveSessionDto>> GetActiveSessionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _store.GetActiveSessionsAsync(userId, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        return sessions.Select(x => new ActiveSessionDto(
            x.Id, x.DeviceName, x.SystemVersion, x.AppVersion, x.IpAddress,
            x.CreatedAt, x.LastActiveAt, x.ExpiresAt)).ToList();
    }

    private async Task<TokenPair> CreateSessionAsync(User user, AuthSessionMetadata metadata, DateTime now, CancellationToken cancellationToken)
    {
        var refreshToken = CreateRefreshToken();
        var expiresAt = now.Add(_options.RefreshTokenLifetime);
        var session = new Session
        {
            UserId = user.Id,
            DeviceName = Limit(metadata.DeviceName, 100, "Unknown device"),
            SystemVersion = Limit(metadata.SystemVersion, 50, "Unknown"),
            AppVersion = Limit(metadata.AppVersion, 50, "Unknown"),
            IpAddress = Limit(metadata.IpAddress, 45, "Unknown"),
            UserAgent = string.IsNullOrWhiteSpace(metadata.UserAgent) ? null : metadata.UserAgent[..Math.Min(metadata.UserAgent.Length, 512)],
            RefreshTokenHash = _refreshTokenHasher.Hash(refreshToken),
            CreatedAt = now,
            LastActiveAt = now,
            ExpiresAt = expiresAt
        };
        await _store.AddSessionAsync(session, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);

        var accessTokenExpiresAt = now.Add(_options.AccessTokenLifetime);
        return new TokenPair(
            _accessTokenIssuer.Create(user.Id, session.Id, accessTokenExpiresAt),
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
