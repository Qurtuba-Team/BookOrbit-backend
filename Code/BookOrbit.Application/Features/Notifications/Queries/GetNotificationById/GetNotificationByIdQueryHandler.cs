namespace BookOrbit.Application.Features.Notifications.Queries.GetNotificationById;

public class GetNotificationByIdQueryHandler(
    ILogger<GetNotificationByIdQueryHandler> logger,
    IAppDbContext context)
    : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(GetNotificationByIdQuery query, CancellationToken ct)
    {
        var notification = await context.Notification
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == query.NotificationId && n.StudentId == query.StudentId, ct);

        if (notification is null)
        {
            logger.LogWarning(
                "Notification {NotificationId} not found for student {StudentId}.",
                query.NotificationId,
                query.StudentId);

            return NotificationApplicationErrors.NotFoundById;
        }

        return NotificationDto.FromEntity(notification);
    }
}
