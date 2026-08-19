namespace Application.Features.Authentication.DataTransferObjects.Requests;

public sealed record RequestCodeRequest(string Email);

public sealed record VerifyCodeRequest(
    string Email,
    string Code,
    string? DeviceName,
    string? SystemVersion,
    string? AppVersion);

public sealed record GoogleSignInRequest(
    string IdToken,
    string? DeviceName,
    string? SystemVersion,
    string? AppVersion);

public sealed record RegisterRequest(
    string RegistrationToken,
    string Username,
    string FirstName,
    string? LastName,
    string? DeviceName,
    string? SystemVersion,
    string? AppVersion);
