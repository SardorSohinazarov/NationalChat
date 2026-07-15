using Application.Features.Authentication;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Stores;

public sealed class EfAuthStore(ChatDb chatdb) : IAuthStore
{
    public Task<EmailVerificationCode?> GetLatestActiveVerificationCodeAsync(string email, VerificationCodePurpose purpose, DateTime now, CancellationToken cancellationToken) =>
        chatdb.EmailVerificationCodes
            .Where(x => x.Email == email && x.Purpose == purpose && x.ConsumedAt == null && x.ExpiresAt > now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<int> CountVerificationCodesAsync(string email, DateTime since, CancellationToken cancellationToken) =>
        chatdb.EmailVerificationCodes.CountAsync(x => x.Email == email && x.CreatedAt >= since, cancellationToken);

    public Task<int> CountVerificationCodesByIpAsync(string ipAddress, DateTime since, CancellationToken cancellationToken) =>
        chatdb.EmailVerificationCodes.CountAsync(x => x.RequestIpAddress == ipAddress && x.CreatedAt >= since, cancellationToken);

    public Task AddVerificationCodeAsync(EmailVerificationCode code, CancellationToken cancellationToken) =>
        chatdb.EmailVerificationCodes.AddAsync(code, cancellationToken).AsTask();

    public Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken) =>
        chatdb.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken) =>
        chatdb.Users.AnyAsync(x => x.Username == username, cancellationToken);

    public Task AddUserAsync(User user, CancellationToken cancellationToken) =>
        chatdb.Users.AddAsync(user, cancellationToken).AsTask();

    public Task AddSessionAsync(Session session, CancellationToken cancellationToken) =>
        chatdb.Sessions.AddAsync(session, cancellationToken).AsTask();

    public Task<Session?> FindSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken) =>
        chatdb.Sessions.FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);

    public Task<Session?> FindSessionAsync(int userId, int sessionId, CancellationToken cancellationToken) =>
        chatdb.Sessions.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == sessionId, cancellationToken);

    public async Task<IReadOnlyList<Session>> GetActiveSessionsAsync(int userId, DateTime now, CancellationToken cancellationToken) =>
        await chatdb.Sessions
            .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
            .OrderByDescending(x => x.LastActiveAt)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        chatdb.SaveChangesAsync(cancellationToken);
}
