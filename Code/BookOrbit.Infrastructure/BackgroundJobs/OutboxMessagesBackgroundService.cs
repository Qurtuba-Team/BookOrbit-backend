namespace BookOrbit.Infrastructure.BackgroundJobs;
public class OutboxMessagesBackgroundService(
        IOptions<BackgroundServicesSettings> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxMessagesBackgroundService> logger,
        TimeProvider timeProvider,
        ISerializationService serializationService,
        IEmailService emailService) : BackgroundService
{
    private readonly BackgroundServicesSettings Options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var timer = new PeriodicTimer(
                TimeSpan.FromMinutes(Options.OutboxMessagesProcessingIntervalInMinutes));

        while (await timer.WaitForNextTickAsync(ct))
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            logger.LogInformation(
                "Starting outbox messages processing at {Time}",
                timeProvider.GetUtcNow());

            var pendingMessages = await context.OutboxMessages
                .Where(m => m.State == Domain.OutboxMessages.Enums.OutboxMessageState.Pending)
                .OrderBy(m => m.CreatedAtUtc)
                .Take(Options.OutboxMessagesBatchSize)
                .ToListAsync(ct);
            
            if(pendingMessages.Count == 0)
            {
                logger.LogInformation("No pending outbox messages found at {Time}", timeProvider.GetUtcNow());
                continue;
            }

            foreach (var message in pendingMessages)
            {
                var deserializedMessageResult = serializationService.Deserialize<OutboxMessageRecord>(message.Payload);

                if (deserializedMessageResult.IsFailure)
                    continue;
                //Logging is beeing done inside service

                var sendEmailResult = await emailService.SendEmailAsync(
                 deserializedMessageResult.Value.EmailAddress,
                 deserializedMessageResult.Value.Subject,
                 deserializedMessageResult.Value.EmailFormat);

                if (sendEmailResult.IsFailure)
                {
                    var markingkAsFailedResult = message.MarkAsFailed();

                    if (markingkAsFailedResult.IsFailure)
                    {
                        logger.LogError(
                            "Failed to mark outbox message with id {MessageId} as failed. Error: {Error}",
                            message.Id,
                            markingkAsFailedResult.TopError);
                    }
                    continue;
                }


                var markingAsProcessedResult = message.MarkAsProcessed();

                if (markingAsProcessedResult.IsFailure)
                {
                    logger.LogError(
                        "Failed to mark outbox message with id {MessageId} as processed. Error: {Error}",
                        message.Id,
                        markingAsProcessedResult.TopError);

                    continue;
                }
            }
            
            await context.SaveChangesAsync(ct);
        }
    }

   
}