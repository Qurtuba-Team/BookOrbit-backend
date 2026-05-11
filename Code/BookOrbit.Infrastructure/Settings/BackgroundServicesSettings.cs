namespace BookOrbit.Infrastructure.Settings
{
    public class BackgroundServicesSettings
    {
        public int ExpirationCheckIntervalInMinutes { get; set; } = 10;

        public int OutboxMessagesProcessingIntervalInMinutes { get; set; } = 1;
        public int OutboxMessagesBatchSize { get; set; } = 50;
    }
}