namespace Application.Features.Authentication;

public interface IOneTimeCodeHasher
{
    string Hash(string code);
    bool Verify(string code, string hash);
}
