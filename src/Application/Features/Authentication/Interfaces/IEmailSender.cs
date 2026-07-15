namespace Application.Features.Authentication;

public interface IEmailSender
{
    Task SendSignInCodeAsync(string email, string code, DateTime expiresAt, CancellationToken cancellationToken);
}
