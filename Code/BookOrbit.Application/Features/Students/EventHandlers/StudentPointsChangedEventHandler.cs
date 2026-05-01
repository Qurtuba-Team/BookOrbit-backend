namespace BookOrbit.Application.Features.Students.EventHandlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using BookOrbit.Domain.Students.DomainEvents;
using BookOrbit.Domain.PointTransactions;

public class StudentPointsChangedEventHandler(
    IAppDbContext context,
    ILogger<StudentPointsChangedEventHandler> logger) : INotificationHandler<StudentPointsChangedEvent>
{
    public Task Handle(StudentPointsChangedEvent notification, CancellationToken cancellationToken)
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
}
