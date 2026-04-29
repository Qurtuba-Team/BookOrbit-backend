using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BookOrbit.Infrastructure.BackgroundJobs;
public class ExpirationEntitiesMarkupService(
    IOptions<BackgroundServicesSettings> options,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ExpirationEntitiesMarkupService> logger,
    TimeProvider timeProvider) : BackgroundService

{
    private readonly BackgroundServicesSettings Options = options.Value;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(Options.ExpirationCheckIntervalInMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogInformation("Starting expiration check for expirable entities at {Time}", timeProvider.GetUtcNow());
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var entityTypes = context.Model.GetEntityTypes()
                .Where(e =>
                    typeof(ExpirableEntity).IsAssignableFrom(e.ClrType) &&
                    !e.IsOwned() &&
                    !e.ClrType.IsAbstract);

            foreach (var entityType in entityTypes)
            {
                var tableName = entityType.GetTableName();
                var schema = entityType.GetSchema();

                var fullTableName = string.IsNullOrEmpty(schema)
                    ? $"[{tableName}]"
                    : $"[{schema}].[{tableName}]";

                var sql = $"""
        UPDATE {fullTableName}
        SET State = @expiredState
        WHERE ExpirationDateUtc IS NOT NULL
          AND ExpirationDateUtc <= @currentTime
          AND State <> @expiredState
    """;
                try
                {
                    var affected = await context.Database.ExecuteSqlRawAsync(
                    sql,
                    new SqlParameter("@expiredState", "Expired"),
                    new SqlParameter("@currentTime", timeProvider.GetUtcNow().UtcDateTime));

                    if (affected > 0)
                    {
                        logger.LogInformation("Marked {Count} records as expired in table {TableName}", affected, fullTableName);
                    }
                    else
                    {
                        logger.LogInformation("No records to mark as expired in table {TableName}", fullTableName);
                    }
                
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while marking records as expired in table {TableName}", fullTableName);
                }

                
            }

        }
    }
}