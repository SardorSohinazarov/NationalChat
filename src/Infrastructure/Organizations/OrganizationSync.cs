using Application.Features.Organizations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Organizations;

/// <summary>
/// Applies the "Organizations" configuration section to the database on startup, before requests are served.
/// A failure is logged and does not stop the application.
/// </summary>
public sealed class OrganizationSync(IServiceScopeFactory scopeFactory, ILogger<OrganizationSync> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var warnings = await scope.ServiceProvider.GetRequiredService<IOrganizationSyncService>().SyncAsync(cancellationToken);
            foreach (var warning in warnings) logger.LogWarning("Tashkilotlar konfiguratsiyasi: {Warning}", warning);
            logger.LogInformation("Tashkilotlar konfiguratsiyasi bazaga yozildi.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Tashkilotlarni sinxronlab bo'lmadi.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
