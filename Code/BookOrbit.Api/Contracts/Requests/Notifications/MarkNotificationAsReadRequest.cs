namespace BookOrbit.Api.Contracts.Requests.Notifications
{
    public record MarkNotificationAsReadRequest
    {
        public DateTime MaxTime { get; init; }
    }
}