namespace BookOrbit.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(
    Guid StudentId,
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = QuerySettings.DefaultSortColumn,
    string? SortDirection = QuerySettings.DefaultSortDirection,
    bool? IsRead = null,
    List<NotificationType>? Types = null)
    : IPagedQuery<NotificationListItemDto>
{
    public string CacheKey => NotificationCachingConstants.NotificationListKey(this);

    public string[] Tags => [NotificationCachingConstants.NotificationTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(NotificationCachingConstants.ExpirationInMinutes);
}
