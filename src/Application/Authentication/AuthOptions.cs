namespace Application.Authentication;

public sealed class AuthOptions
{
    public TimeSpan CodeLifetime { get; init; } = TimeSpan.FromMinutes(10);
    public TimeSpan ResendInterval { get; init; } = TimeSpan.FromMinutes(1);
    public TimeSpan RequestWindow { get; init; } = TimeSpan.FromMinutes(15);
    public int MaxRequestsPerWindow { get; init; } = 5;
    public int MaxCodeAttempts { get; init; } = 5;
    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);
    public TimeSpan RefreshTokenLifetime { get; init; } = TimeSpan.FromDays(30);
    public TimeSpan RegistrationTokenLifetime { get; init; } = TimeSpan.FromMinutes(10);
}
