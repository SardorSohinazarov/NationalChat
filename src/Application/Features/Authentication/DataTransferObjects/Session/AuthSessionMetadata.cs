namespace Application.Features.Authentication.DataTransferObjects.Session;

public sealed record AuthSessionMetadata(
    string DeviceName,
    string SystemVersion,
    string AppVersion,
    string IpAddress,
    string? UserAgent);
