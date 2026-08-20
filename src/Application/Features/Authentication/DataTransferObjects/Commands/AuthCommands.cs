using Application.Features.Authentication.DataTransferObjects.Session;

namespace Application.Features.Authentication.DataTransferObjects.Commands;

public sealed record RequestSignInCodeCommand(string Email, string IpAddress);

public sealed record VerifySignInCodeCommand(string Email, string Code, AuthSessionMetadata Session);

public sealed record GoogleSignInCommand(string IdToken, AuthSessionMetadata Session);

public sealed record CompleteRegistrationCommand(
    string RegistrationToken,
    string Username,
    string FirstName,
    string? LastName,
    AuthSessionMetadata Session);

public sealed record RefreshSessionCommand(string RefreshToken, string IpAddress);
