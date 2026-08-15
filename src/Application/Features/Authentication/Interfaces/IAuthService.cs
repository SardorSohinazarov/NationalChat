using Application.Features.Authentication.DataTransferObjects.Commands;
using Application.Features.Authentication.DataTransferObjects.Responses;

namespace Application.Features.Authentication;

public interface IAuthService
{
    Task<SignInCodeRequestResult> RequestSignInCodeAsync(RequestSignInCodeCommand command, CancellationToken cancellationToken = default);
    Task<SignInVerificationResult> VerifySignInCodeAsync(VerifySignInCodeCommand command, CancellationToken cancellationToken = default);
    Task<RegistrationResult> CompleteRegistrationAsync(CompleteRegistrationCommand command, CancellationToken cancellationToken = default);
    Task<TokenPair?> RefreshSessionAsync(RefreshSessionCommand command, CancellationToken cancellationToken = default);
    Task LogoutAsync(int userId, int sessionId, CancellationToken cancellationToken = default);
    Task LogoutAllAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(int userId, int sessionId, int currentSessionId, CancellationToken cancellationToken = default);
    Task LogoutAllOthersAsync(int userId, int currentSessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActiveSessionDto>> GetActiveSessionsAsync(int userId, int currentSessionId, CancellationToken cancellationToken = default);
}
