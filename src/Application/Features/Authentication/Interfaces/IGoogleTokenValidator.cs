namespace Application.Features.Authentication;

public sealed record GoogleIdentity(string Email, bool EmailVerified);

public interface IGoogleTokenValidator
{
    Task<GoogleIdentity?> ValidateAsync(string idToken, CancellationToken cancellationToken);
}
