namespace Application.Authentication;

public sealed record AuthSessionMetadata(
    string DeviceName,
    string SystemVersion,
    string AppVersion,
    string IpAddress,
    string? UserAgent);

public sealed record RequestSignInCodeCommand(string Email, string IpAddress);

public sealed record VerifySignInCodeCommand(string Email, string Code, AuthSessionMetadata Session);

public sealed record CompleteRegistrationCommand(
    string RegistrationToken,
    string Username,
    string FirstName,
    string? LastName,
    AuthSessionMetadata Session);

public sealed record RefreshSessionCommand(string RefreshToken, string IpAddress);

public sealed record TokenPair(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);

public sealed record SignInCodeRequestResult(bool IsAccepted, DateTime? RetryAfter, string? Error);

public sealed record SignInVerificationResult(
    bool IsSuccessful,
    bool RegistrationRequired,
    string? RegistrationToken,
    TokenPair? Tokens,
    string? Error);

public sealed record RegistrationResult(bool IsSuccessful, TokenPair? Tokens, string? Error);

public sealed record ActiveSessionDto(
    int Id,
    string DeviceName,
    string SystemVersion,
    string AppVersion,
    string IpAddress,
    DateTime CreatedAt,
    DateTime LastActiveAt,
    DateTime ExpiresAt);

public interface IOneTimeCodeHasher
{
    string Hash(string code);
    bool Verify(string code, string hash);
}

public interface IRefreshTokenHasher
{
    string Hash(string token);
}

public interface IAccessTokenIssuer
{
    string Create(int userId, int sessionId, DateTime expiresAt);
}

public interface IRegistrationTokenService
{
    string Create(string email, DateTime expiresAt);
    string? Validate(string token, DateTime now);
}

public interface IEmailSender
{
    Task SendSignInCodeAsync(string email, string code, DateTime expiresAt, CancellationToken cancellationToken);
}
