using Application.Features.Authentication.DataTransferObjects.Session;
using Domain.Entities;

namespace Application.Features.Authentication.Factories;

public static class AuthEntityFactory
{
    public static User CreateUser(string email, string username, string firstName, string? lastName, DateTime createdAt) =>
        new()
        {
            Email = email,
            Username = username,
            FirstName = firstName.Trim(),
            LastName = NormalizeOptional(lastName),
            IsProfileCompleted = true,
            CreatedAt = createdAt
        };

    public static Session CreateSession(int userId, AuthSessionMetadata metadata, string refreshTokenHash, DateTime now, DateTime expiresAt) =>
        new()
        {
            UserId = userId,
            DeviceName = Limit(metadata.DeviceName, 100, "Unknown device"),
            SystemVersion = Limit(metadata.SystemVersion, 50, "Unknown"),
            AppVersion = Limit(metadata.AppVersion, 50, "Unknown"),
            IpAddress = Limit(metadata.IpAddress, 45, "Unknown"),
            UserAgent = string.IsNullOrWhiteSpace(metadata.UserAgent) ? null : metadata.UserAgent[..Math.Min(metadata.UserAgent.Length, 512)],
            RefreshTokenHash = refreshTokenHash,
            CreatedAt = now,
            LastActiveAt = now,
            ExpiresAt = expiresAt
        };

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Limit(string value, int maxLength, string fallback)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized[..Math.Min(normalized.Length, maxLength)];
    }
}
