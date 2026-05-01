
using BookOrbit.Application.Features.BorrowingRequests.Dtos;
using BookOrbit.Domain.Students;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsLostBorrowingTransaction;
public class MarkAsLostBorrowingTransactionCommandHandler(
    IAppDbContext context,
    ILogger<MarkAsLostBorrowingTransactionCommandHandler> logger)
    : IRequestHandler<MarkAsLostBorrowingTransactionCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MarkAsLostBorrowingTransactionCommand command, CancellationToken ct)
    {
        var transactionData = await context.BorrowingTransactions
            .Select(bt => new
            {
                BorrowingTransaction = bt,
                BookCopy = bt!.BookCopy
            })
            .FirstOrDefaultAsync(bt => bt.BorrowingTransaction.Id == command.BorrowingTransactionId, ct);

        if (transactionData is null)
        {
            logger.LogWarning(
                "Borrowing transaction {BorrowingTransactionId} not found for marking as lost.",
                command.BorrowingTransactionId);

            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        var lostResult = transactionData.BorrowingTransaction.MarkAsLost();

        if (lostResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to mark borrowing transaction {BorrowingTransactionId} as lost. Errors: {Errors}",
                command.BorrowingTransactionId,
                string.Join(", ", lostResult.Errors.Select(e => e.Code)));

            return lostResult.Errors;
        }

        var updateBookCopyResult = transactionData.BookCopy!.MarkAsLost();

        if (updateBookCopyResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to mark book copy {BookCopyId} as lost for borrowing transaction {BorrowingTransactionId}. Errors: {Errors}",
                transactionData.BookCopy.Id,
                command.BorrowingTransactionId,
                string.Join(", ", updateBookCopyResult.Errors.Select(e => e.Code)));

            return updateBookCopyResult.Errors;
        }

        var logCreationResult = BorrowingTransactionEvent.Create(
            Guid.NewGuid(),
            transactionData.BorrowingTransaction.Id,
            transactionData.BorrowingTransaction.State);

        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == transactionData.BorrowingTransaction.BorrowerStudentId, cancellationToken: ct);

        if (student is null)
        {
            logger.LogWarning(
                "Student {StudentId} not found for borrowing transaction {LendingRecordId}.",
                transactionData.BorrowingTransaction.BorrowerStudentId,
                transactionData.BorrowingTransaction.Id);
            return StudentApplicationErrors.NotFoundById;
        }

        var pointsToDeductCreationResult = Point.Create(Point.LostBookPenalty);

        if(pointsToDeductCreationResult.IsFailure)
        {
            logger.LogWarning(
    "Failed to create points for student {StudentId}. Errors: {Errors}",
    student.Id,
    pointsToDeductCreationResult.Errors);
            return pointsToDeductCreationResult.Errors;
        }

        student.DeductPoints(pointsToDeductCreationResult.Value);

        var pointLogCreationResult = PointTransaction.Create(
            Guid.NewGuid(),
            student.Id,
            null,
            pointsToDeductCreationResult.Value.Value,
            PointTransactionReason.Penalty);

        if (pointLogCreationResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create point transaction for student {StudentId} for borrowing transaction {BorrowingTransactionId}. Errors: {Errors}",
                student.Id,
                transactionData.BorrowingTransaction.Id,
                pointLogCreationResult.Errors);
            return pointLogCreationResult.Errors;
        }


        context.BorrowingTransactionEvents.Add(logCreationResult.Value);
        context.PointTransactions.Add(pointLogCreationResult.Value);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} has been marked as lost.",
            transactionData.BorrowingTransaction.Id);

        return Result.Updated;
    }
}
