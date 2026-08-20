using Application.Features.Authentication;
using global::Google.Apis.Auth;

namespace Infrastructure.Security.Google;

public sealed class GoogleTokenValidator(GoogleAuthOptions options) : IGoogleTokenValidator
{
    public async Task<GoogleIdentity?> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [options.ClientId]
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return new GoogleIdentity(payload.Email, payload.EmailVerified);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
