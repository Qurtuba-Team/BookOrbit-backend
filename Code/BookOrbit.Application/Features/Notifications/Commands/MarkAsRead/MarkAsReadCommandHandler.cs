
namespace BookOrbit.Application.Features.Notifications.Commands.MarkAsRead;
public class MarkAsReadCommandHandler(
    ILogger<MarkAsReadCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<MarkAsReadCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MarkAsReadCommand request, CancellationToken ct)
    {
        var isStudentExists = await context.Students.AnyAsync(s => s.Id == request.StudentId, cancellationToken: ct);

        if(!isStudentExists)
        {
            logger.LogWarning("Student with id {StudentId} not found", request.StudentId);
            return StudentApplicationErrors.NotFoundById;
        }

        var universalTime = request.MaxTime.ToUniversalTime();

       var affectedRows = await context.Notification
            .Where(n => n.CreatedAtUtc <= universalTime
                     && n.StudentId == request.StudentId
                     && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true),
                ct);

        logger.LogInformation("Marked {Count} notifications as read", affectedRows);

        await cache.RemoveByTagAsync(NotificationCachingConstants.NotificationTag, ct);
        return Result.Updated;
    }
}