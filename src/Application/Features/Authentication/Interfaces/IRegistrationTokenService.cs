namespace Application.Features.Authentication;

public interface IRegistrationTokenService
{
    string Create(string email, DateTime expiresAt);
    string? Validate(string token, DateTime now);
}
