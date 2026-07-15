namespace Application.Features.Authentication;

public interface IAccessTokenIssuer
{
    string Create(int userId, int sessionId, DateTime expiresAt);
}
