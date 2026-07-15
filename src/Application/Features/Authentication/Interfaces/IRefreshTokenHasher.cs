namespace Application.Features.Authentication;

public interface IRefreshTokenHasher
{
    string Hash(string token);
}
