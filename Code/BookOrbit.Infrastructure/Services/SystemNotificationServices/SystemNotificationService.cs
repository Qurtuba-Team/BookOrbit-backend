using BookOrbit.Domain.Notifications.Enums;

namespace BookOrbit.Infrastructure.Services.SystemNotificationServices;
public class SystemNotificationService(
    ILogger<SystemNotificationService> logger,
    IAppDbContext context,
    HybridCache cache) : ISystemNotificationService
{
    public async Task SendNotificationAsync(Guid studentId, string title, string message, NotificationType type, CancellationToken cancellationToken = default)
    {
        var studentExists = await context.Students.AnyAsync(s => s.Id == studentId, cancellationToken: cancellationToken);

        if(!studentExists)
        {
            logger.LogWarning("Attempted to send notification to non-existent student with ID: {StudentId}", studentId);
            return;
        }

        var notificationCreationResult = Notification.Create(Guid.NewGuid(),studentId, title, message, type);

        if(notificationCreationResult.IsFailure)
        {
            logger.LogError("Failed to create notification for student ID: {StudentId}. Errors: {Errors}", studentId, notificationCreationResult.Errors);
            return;
        }

        context.Notification.Add(notificationCreationResult.Value);
        //Save Changes in caller
        await cache.RemoveByTagAsync(NotificationCachingConstants.NotificationTag, cancellationToken);
    }
}