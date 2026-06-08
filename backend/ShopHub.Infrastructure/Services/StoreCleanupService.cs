using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Domain.Enums;

namespace ShopHub.Infrastructure.Services;

public class StoreCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<StoreCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in StoreCleanupService.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IShopHubDbContext>();
        var kubernetes = scope.ServiceProvider.GetRequiredService<IKubernetesService>();

        var stores = await db.Stores
            .Where(s => s.Status == StoreStatus.Deleting)
            .ToListAsync(cancellationToken);

        foreach (var store in stores)
        {
            try
            {
                await kubernetes.DeleteShopAsync(store, cancellationToken);
                await kubernetes.DeleteDatabaseAsync(store, cancellationToken);
                db.Stores.Remove(store);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Store {StoreId} ({Name}) successfully deleted.", store.Id, store.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to cleanup store {StoreId}. Will retry on next run.", store.Id);
            }
        }
    }
}
