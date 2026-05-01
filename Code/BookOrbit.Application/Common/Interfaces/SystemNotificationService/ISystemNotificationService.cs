namespace BookOrbit.Application.Common.Interfaces.SystemNotificationService;
public interface ISystemNotificationService
{
    Task SendNotificationAsync(Guid studentId, string title,string message,NotificationType type, CancellationToken cancellationToken = default);
}