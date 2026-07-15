using Application.Authentication;

namespace API.Email;

public sealed class DevelopmentEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendSignInCodeAsync(string email, string code, DateTime expiresAt, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Development sign-in code for {Email}: {Code}; expires at {ExpiresAtUtc}", email, code, expiresAt);
        return Task.CompletedTask;
    }
}
