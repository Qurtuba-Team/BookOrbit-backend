using BookOrbit.Application.Common.Interfaces.SystemNotificationService;

namespace BookOrbit.Application.Features.BorrowingRequests.EventHandlers;
public class BorrowingRequestTerminatedEventHandler(
    IAppDbContext context,
    ILogger<BorrowingRequestTerminatedEventHandler> logger,
    ISystemNotificationService systemNotificationService) : INotificationHandler<BorrowingRequestTerminatedEvent>
{
    private async Task Refund(BorrowingRequestTerminatedEvent notification,Student BorroiwngStudent, CancellationToken ct)
    {
        var lendingRecord = await context.LendingListRecords.FirstOrDefaultAsync(lr => lr.Id == notification.LendingRecordId, ct);
        if (lendingRecord is null)
        {
            logger.LogWarning("LendingRecord {LendingRecordId} not found in BorrowingRequestTerminatedEventHandler.", notification.LendingRecordId);
            return;
        }


        // Refund points
        var pointCreationResult = Point.Create(lendingRecord.Cost.Value);
        if (pointCreationResult.IsSuccess)
        {
            BorroiwngStudent.AddPoints(pointCreationResult.Value, PointTransactionReason.Refund);
        }

        // Mark listing as available
        lendingRecord.MarkAsAvailable();

        logger.LogInformation(
            "Refunded points and marked lending record {LendingRecordId} available due to termination of borrowing request {BorrowingRequestId}.",
            notification.LendingRecordId,
            notification.BorrowingRequestId);
    }

    private async Task NotifyLendingStudent(Guid studentId, BorrowingRequestState state, CancellationToken ct)
    {
        string title = $"A borrowing request that was sent to you has been {state} ";
        string message = $"A borrowing request that was sent to you has been {state} ";

        await systemNotificationService.SendNotificationAsync(studentId, title, message, NotificationType.Normal, ct);
    }
    private async Task NotifyBorrowingStudent(Guid studentId, BorrowingRequestState state, CancellationToken ct)
    {
        string title = $"Your borrowing request has been {state} ";
        string message = $"Your borrowing request has been {state} ";

        await systemNotificationService.SendNotificationAsync(studentId, title, message, NotificationType.Normal, ct);
    }



    public async Task Handle(BorrowingRequestTerminatedEvent notification, CancellationToken ct)
    {
        var BorroiwngStudent = await context.Students.FirstOrDefaultAsync(s => s.Id == notification.BorrowingStudentId, ct);
        if (BorroiwngStudent is null)
        {
            logger.LogWarning("Student {StudentId} not found in BorrowingRequestTerminatedEventHandler.", notification.BorrowingStudentId);
            return;
        }

        var LendingStudentId = await context.BorrowingRequests.
            Where(br => br.Id == notification.BorrowingRequestId)
            .Select(br=>new
            {
                LendingStudentId = br.LendingRecord!.BookCopy!.OwnerId
            })
            .FirstOrDefaultAsync(ct);

        if (LendingStudentId is null)
        {
            logger.LogWarning("LendingStudentId for BorrowingRequest {BorrowingRequestId} not found in BorrowingRequestTerminatedEventHandler.", notification.BorrowingRequestId);
            return;
        }

        await Refund(notification, BorroiwngStudent, ct);

        await NotifyLendingStudent(LendingStudentId.LendingStudentId, notification.State, ct);
        await NotifyBorrowingStudent(BorroiwngStudent.Id, notification.State, ct);   
    }
}