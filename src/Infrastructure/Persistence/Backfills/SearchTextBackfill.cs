using Application.Features.Messages.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Backfills;

/// <summary>
/// Fills <c>messages.SearchText</c> for messages written before script-independent search existed.
/// Runs once on startup in batches; it only touches rows where SearchText is still NULL, so restarts are safe.
/// </summary>
public sealed class SearchTextBackfill(IServiceScopeFactory scopeFactory, ILogger<SearchTextBackfill> logger) : BackgroundService
{
    private const int BatchSize = 500;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var updated = await RunAsync(stoppingToken);
            if (updated > 0) logger.LogInformation("SearchText backfill: {Count} ta xabar yangilandi.", updated);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "SearchText backfill bajarilmadi.");
        }
    }

    private async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDb>();
        var updated = 0;
        var lastId = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Keyset over Id: rows whose text normalizes to nothing stay NULL, so "WHERE SearchText IS NULL"
            // alone would return them again and never finish.
            var batch = await db.Messages.IgnoreQueryFilters()
                .Where(message => message.Id > lastId && message.SearchText == null && message.TextContent != null && message.ServiceAction == null)
                .OrderBy(message => message.Id)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
            if (batch.Count == 0) break;

            foreach (var message in batch)
            {
                message.SearchText = MessageFactory.BuildSearchText(message.TextContent);
            }

            updated += await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
            lastId = batch[^1].Id;
        }

        return updated;
    }
}
