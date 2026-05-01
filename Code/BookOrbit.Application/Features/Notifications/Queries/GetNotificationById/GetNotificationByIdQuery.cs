namespace BookOrbit.Application.Features.Notifications.Queries.GetNotificationById;

public record GetNotificationByIdQuery(Guid StudentId, Guid NotificationId)
    : ICachedQuery<Result<NotificationDto>>
{
    public string CacheKey => NotificationCachingConstants.NotificationKey(StudentId, NotificationId);

    public string[] Tags => [NotificationCachingConstants.NotificationTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(NotificationCachingConstants.ExpirationInMinutes);
}
