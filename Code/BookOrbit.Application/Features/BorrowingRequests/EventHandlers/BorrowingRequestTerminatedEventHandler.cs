namespace BookOrbit.Application.Features.BorrowingRequests.EventHandlers;
public class BorrowingRequestTerminatedEventHandler(
    IAppDbContext context,
    ILogger<BorrowingRequestTerminatedEventHandler> logger) : INotificationHandler<BorrowingRequestTerminatedEvent>
{
    public async Task Handle(BorrowingRequestTerminatedEvent notification, CancellationToken cancellationToken)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == notification.BorrowingStudentId, cancellationToken);
        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found in BorrowingRequestTerminatedEventHandler.", notification.BorrowingStudentId);
            return;
        }

        var lendingRecord = await context.LendingListRecords.FirstOrDefaultAsync(lr => lr.Id == notification.LendingRecordId, cancellationToken);
        if (lendingRecord is null)
        {
            logger.LogWarning("LendingRecord {LendingRecordId} not found in BorrowingRequestTerminatedEventHandler.", notification.LendingRecordId);
            return;
        }

        // Refund points
        var pointCreationResult = Point.Create(lendingRecord.Cost.Value);
        if (pointCreationResult.IsSuccess)
        {
            student.AddPoints(pointCreationResult.Value, PointTransactionReason.Refund);
        }

        // Mark listing as available
        lendingRecord.MarkAsAvailable();

        logger.LogInformation(
            "Refunded points and marked lending record {LendingRecordId} available due to termination of borrowing request {BorrowingRequestId}.",
            notification.LendingRecordId,
            notification.BorrowingRequestId);
    }
}