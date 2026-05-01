namespace BookOrbit.Api.Contracts.Requests.Notifications;

public record NotificationPagedFilterRequest : PagedFilterRequest
{
    public bool? IsRead { get; set; } = null;
    public List<NotificationType>? Types { get; set; } = null;
}
