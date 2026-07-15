using Domain.Entities;

namespace Application.Authentication;

public interface IAuthStore
{
    Task<EmailVerificationCode?> GetLatestActiveVerificationCodeAsync(string email, VerificationCodePurpose purpose, DateTime now, CancellationToken cancellationToken);
    Task<int> CountVerificationCodesAsync(string email, DateTime since, CancellationToken cancellationToken);
    Task<int> CountVerificationCodesByIpAsync(string ipAddress, DateTime since, CancellationToken cancellationToken);
    Task AddVerificationCodeAsync(EmailVerificationCode code, CancellationToken cancellationToken);
    Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken);
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task AddSessionAsync(Session session, CancellationToken cancellationToken);
    Task<Session?> FindSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken);
    Task<Session?> FindSessionAsync(int userId, int sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Session>> GetActiveSessionsAsync(int userId, DateTime now, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
