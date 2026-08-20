using Application.Features.Authentication;

namespace Infrastructure.Email;

public sealed class CompositeEmailSender(
    DevelopmentEmailSender consoleSender,
    SmtpEmailSender smtpSender) : IEmailSender
{
    public async Task SendSignInCodeAsync(string email, string code, DateTime expiresAt, CancellationToken cancellationToken)
    {
        await consoleSender.SendSignInCodeAsync(email, code, expiresAt, cancellationToken);
        await smtpSender.SendSignInCodeAsync(email, code, expiresAt, cancellationToken);
    }
}
