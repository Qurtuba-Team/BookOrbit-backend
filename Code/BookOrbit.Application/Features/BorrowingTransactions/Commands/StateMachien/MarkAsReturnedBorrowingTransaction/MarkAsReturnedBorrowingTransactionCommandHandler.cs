
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


        var student = await context.Students.FirstOrDefaultAsync(s=>s.Id == transaction.borrowingTransaction.BorrowerStudentId, ct);

        if(student is null)
        {
            logger.LogWarning(
                "Student {StudentId} not found for borrowing transaction {BorrowingTransactionId}.",
                transaction.borrowingTransaction.BorrowerStudentId,
                transaction.borrowingTransaction.Id);
            return StudentApplicationErrors.NotFoundById;
        }

        var pointsToAddCreationResult = Point.Create(Point.ReturningBookReward);

        if(pointsToAddCreationResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create points for student {StudentId}. Errors: {Errors}",
                student.Id,
                pointsToAddCreationResult.Errors);
            return pointsToAddCreationResult.Errors;
        }

        
        var pointAdditionResult = student.AddPoints(pointsToAddCreationResult.Value, PointTransactionReason.Returning);

        if (pointAdditionResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to add points to student {StudentId} for borrowing transaction {BorrowingTransactionId}. Errors: {Errors}",
                student.Id,
                transaction.borrowingTransaction.Id,
                pointAdditionResult.Errors);
            return pointAdditionResult.Errors;
        }


        await context.SaveChangesAsync(ct);
        await hybridCache.RemoveByTagAsync(BorrowingTransactionCachingConstants.BorrowingTransactionTag, ct);


        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} has been marked as returned.",
            transaction.borrowingTransaction.Id);

        return Result.Updated;
    }
}
