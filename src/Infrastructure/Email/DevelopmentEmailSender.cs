using Application.Features.Authentication;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

public sealed class DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger) : IEmailSender
{
    public Task SendSignInCodeAsync(string email, string code, DateTime expiresAt, CancellationToken cancellationToken)
    {
        logger.LogWarning("Development sign-in code for {Email}: {Code}; expires at {ExpiresAtUtc}", email, code, expiresAt);
        return Task.CompletedTask;
    }
}
