namespace BookOrbit.Api.Contracts.Requests.Notifications
{
    public record MarkNotificationAsReadRequest
    {
        public DateTimeOffset MaxTime { get; init; }
    }
}