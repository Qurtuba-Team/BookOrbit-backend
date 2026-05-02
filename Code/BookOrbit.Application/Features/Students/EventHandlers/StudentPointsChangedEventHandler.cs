namespace BookOrbit.Application.Features.Students.EventHandlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using BookOrbit.Domain.Students.DomainEvents;
using BookOrbit.Domain.PointTransactions;
using BookOrbit.Application.Common.Interfaces.SystemNotificationService;

public class StudentPointsChangedEventHandler(
    IAppDbContext context,
    ILogger<StudentPointsChangedEventHandler> logger,
    ISystemNotificationService systemNotificationService) : INotificationHandler<StudentPointsChangedEvent>
{
    private Task AddTransactionLog(StudentPointsChangedEvent notification)
    {
        var pointTransactionResult = PointTransaction.Create(
            Guid.NewGuid(),
            notification.StudentId,
            notification.BorrowingReviewId,
            notification.Points,
            notification.Reason);

        if (pointTransactionResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create point transaction for student {StudentId} during event handling. Errors: {Errors}",
                notification.StudentId,
                pointTransactionResult.Errors);
            return Task.CompletedTask;
        }

        context.PointTransactions.Add(pointTransactionResult.Value);

        logger.LogInformation(
            "Point transaction created for student {StudentId}. Points: {Points}, Reason: {Reason}",
            notification.StudentId,
            notification.Points,
            notification.Reason);

        return Task.CompletedTask;
    }
    public async Task Notify(StudentPointsChangedEvent notification, CancellationToken ct)
    {
        if(notification.Points == 0)
            return ;

        string verb = notification.Points > 0 ? "earned" : "lost";
        NotificationType notificationType = notification.Points > 0 ? NotificationType.Good : NotificationType.Bad;

        string title = $"You have {verb} points";
        string message = $"You have {verb} {Math.Abs(notification.Points)} points";

        await systemNotificationService.SendNotificationAsync(notification.StudentId, title, message,
            notificationType, ct);
    }

    public async Task Handle(StudentPointsChangedEvent notification, CancellationToken cancellationToken)
    {
        await AddTransactionLog(notification);
        await Notify(notification, cancellationToken);
    }
}
