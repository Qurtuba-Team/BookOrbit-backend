
namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsReturnedBorrowingTransaction;
public class MarkAsReturnedBorrowingTransactionCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider,
    ILogger<MarkAsReturnedBorrowingTransactionCommandHandler> logger,
    HybridCache hybridCache)
    : IRequestHandler<MarkAsReturnedBorrowingTransactionCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MarkAsReturnedBorrowingTransactionCommand command, CancellationToken ct)
    {
        var transaction = await context.BorrowingTransactions
            .Select(bt =>
            new
            {
                borrowingTransaction = bt,
                bookCopy = bt.BookCopy
            })
            .FirstOrDefaultAsync(bt => bt.borrowingTransaction.Id == command.BorrowingTransactionId, ct);

        if (transaction is null)
        {
            logger.LogWarning(
                "Borrowing transaction {BorrowingTransactionId} not found for marking as returned.",
                command.BorrowingTransactionId);

            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        var now = timeProvider.GetUtcNow();
        var returnResult = transaction.borrowingTransaction.ReturnBookCopy(now, now);

        if (returnResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to mark borrowing transaction {BorrowingTransactionId} as returned. Errors: {Errors}",
                command.BorrowingTransactionId,
                string.Join(", ", returnResult.Errors.Select(e => e.Code)));

            return returnResult.Errors;
        }

        var updateBookCopyResult = transaction.bookCopy!.MarkAsAvilable();

        if(updateBookCopyResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to mark book copy as available for borrowing transaction {BorrowingTransactionId}. Errors: {Errors}",
                command.BorrowingTransactionId,
                string.Join(", ", updateBookCopyResult.Errors.Select(e => e.Code)));

            return updateBookCopyResult.Errors;
        }

        var logCreationResult = BorrowingTransactionEvent.Create(
            Guid.NewGuid(),
            transaction.borrowingTransaction.Id,
            transaction.borrowingTransaction.State);

        var student = await context.Students.FirstOrDefaultAsync(s=>s.Id == transaction.borrowingTransaction.BorrowerStudentId, ct);

        if(student is null)
        {
            logger.LogWarning(
                "Student {StudentId} not found for borrowing transaction {BorrowingTransactionId}.",
                transaction.borrowingTransaction.BorrowerStudentId,
                transaction.borrowingTransaction.Id);
            return StudentApplicationErrors.NotFoundById;
        }

        var pointAdditionResult = student.AddPoints(Point.Create(2).Value);

        if (pointAdditionResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to add points to student {StudentId} for borrowing transaction {BorrowingTransactionId}. Errors: {Errors}",
                student.Id,
                transaction.borrowingTransaction.Id,
                pointAdditionResult.Errors);
            return pointAdditionResult.Errors;
        }

        var pointTransactionResult = PointTransaction.Create(
            Guid.NewGuid(),
            student.Id,
            null,
            2,
            PointTransactionReason.Returning);

        context.PointTransactions.Add(pointTransactionResult.Value);
        context.BorrowingTransactionEvents.Add(logCreationResult.Value);

        await context.SaveChangesAsync(ct);
        await hybridCache.RemoveByTagAsync(BorrowingTransactionCachingConstants.BorrowingTransactionTag, ct);


        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} has been marked as returned.",
            transaction.borrowingTransaction.Id);

        return Result.Updated;
    }
}
