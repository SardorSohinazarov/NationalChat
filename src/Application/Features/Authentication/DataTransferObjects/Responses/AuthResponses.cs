namespace Application.Features.Authentication.DataTransferObjects.Responses;

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
