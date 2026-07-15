namespace Infrastructure.Security.Options;

public sealed class AuthSecurityOptions
{
    public string RefreshTokenHashKey { get; init; } = string.Empty;
    public string RegistrationTokenSigningKey { get; init; } = string.Empty;
}
