using BookOrbit.Domain.BorrowingTransactions.DomainEvents;

namespace BookOrbit.Application.Features.BorrowingTransactions.EventHandlers;
public class BorrowingTransactionBookCopyReturnedEventHandler(
    ILogger<BorrowingTransactionBookCopyReturnedEventHandler> logger,
    IAppDbContext context) : INotificationHandler<BorrowingTransactionBookCopyReturnedEvent>
{
    public async Task Handle(BorrowingTransactionBookCopyReturnedEvent notification, CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == notification.BorrowingStudentId, ct);
        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found in BorrowingRequestTerminatedEventHandler.", notification.BorrowingStudentId);
            return;
        }


        if(notification.State is BorrowingTransactionState.Returned)
        {
            var pointsToAdd = Point.Create(Point.ReturningBookReward);
            var addingPointsResult = student.AddPoints(pointsToAdd.Value, PointTransactionReason.Returning, notification.BorrowingTransactionId);
            if(addingPointsResult.IsFailure)
            {
                logger.LogError("Failed to add points to student {StudentId} for returning borrowing transaction {BorrowingTransactionId}. Error: {Error}", student.Id, notification.BorrowingTransactionId, string.Join(", ", addingPointsResult.Errors.Select(e => e.Code)));
                return;
            }
        }


        else if(notification.State is BorrowingTransactionState.Overdue)
        {
            var pointsToDeduct = Point.Create(Point.OverduePenalty);
            var deductPointsResult = student.DeductPoints(pointsToDeduct.Value, PointTransactionReason.Penalty, notification.BorrowingTransactionId);
            if (deductPointsResult.IsFailure)
            {
                logger.LogError("Failed to deduct points from student {StudentId} for overdue return of borrowing transaction {BorrowingTransactionId}. Error: {Error}", student.Id, notification.BorrowingTransactionId, string.Join(", ", deductPointsResult.Errors.Select(e => e.Code)));
                return;
            }
        }
    }
}
