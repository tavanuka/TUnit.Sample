using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TUnit.Sample.Infrastructure;

namespace TUnit.Sample.ApiService.Workers;

public class DatabaseWorker(IServiceProvider sp) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    internal const string ActivitySourceName = "Data operations";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(DatabaseWorker));

        await using var scope = sp.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();

        try
        {
            await TryEnsureCreatedAsync(dbContext, stoppingToken);

        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.AddException(ex);
            throw;
        }
        
    }

    internal static async Task TryEnsureCreatedAsync(DbContext dbContext, CancellationToken stoppingToken)
    {
        // When no database is created, this will trigger async seeding if it doesn't exist for development purposes.
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(
            dbContext,
            async static (db, ct) => await db.Database.EnsureCreatedAsync(ct),
            stoppingToken
        );
    }
}