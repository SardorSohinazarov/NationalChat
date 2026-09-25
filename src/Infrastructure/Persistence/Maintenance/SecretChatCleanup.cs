using Application.Features.SecretChats;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Maintenance;

/// <summary>
/// Hourly: closes secret chats whose device session expired or was revoked, closes requests nobody accepted,
/// and drops undelivered ciphertext past its retention. The rules live in <see cref="ISecretChatService.CleanupAsync"/>.
/// </summary>
public sealed class SecretChatCleanup(IServiceScopeFactory scopeFactory, ILogger<SecretChatCleanup> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ISecretChatService>().CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Maxfiy chatlarni tozalash bajarilmadi.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
