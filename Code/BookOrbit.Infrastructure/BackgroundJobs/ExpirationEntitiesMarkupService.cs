
namespace BookOrbit.Infrastructure.BackgroundJobs
{
    public class ExpirationEntitiesMarkupService(
        IOptions<BackgroundServicesSettings> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ExpirationEntitiesMarkupService> logger,
        TimeProvider timeProvider) : BackgroundService
    {
        private readonly BackgroundServicesSettings Options = options.Value;

        private static readonly Dictionary<Type, object> ExpirationStateConditions = new()
    {
        { typeof(BorrowingRequest), BorrowingRequestState.Pending },
        { typeof(LendingListRecord), LendingListRecordState.Available }
    };

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(
                TimeSpan.FromMinutes(Options.ExpirationCheckIntervalInMinutes));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                using var scope = serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                logger.LogInformation(
                    "Starting expiration check at {Time}",
                    timeProvider.GetUtcNow());

                var entityTypes = context.Model.GetEntityTypes()
                    .Where(e =>
                        typeof(ExpirableEntity).IsAssignableFrom(e.ClrType) &&
                        !e.IsOwned() &&
                        !e.ClrType.IsAbstract);

                foreach (var entityType in entityTypes)
                {
                    var clrType = entityType.ClrType;

                    if (!ExpirationStateConditions.TryGetValue(clrType, out var requiredState))
                        continue;

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
                      AND State = @requiredState
                """;

                    var affected = await context.Database.ExecuteSqlRawAsync(
                        sql,
                        new SqlParameter("@expiredState", "Expired"),
                        new SqlParameter("@requiredState", requiredState.ToString()),
                        new SqlParameter("@currentTime", timeProvider.GetUtcNow().UtcDateTime)
                    );

                    if (affected > 0)
                    {
                        logger.LogInformation(
                            "Updated {Count} rows in {Table} from {FromState} to Expired",
                            affected,
                            tableName,
                            requiredState);
                    }
                }
            }
        }
    }
}