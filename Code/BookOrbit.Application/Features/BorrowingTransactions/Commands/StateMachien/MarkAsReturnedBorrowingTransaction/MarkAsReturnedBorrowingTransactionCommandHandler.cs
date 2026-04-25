using BookOrbit.Application.Features.BorrowingTransactions;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsReturnedBorrowingTransaction;
public class MarkAsReturnedBorrowingTransactionCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider,
    ILogger<MarkAsReturnedBorrowingTransactionCommandHandler> logger)
    : IRequestHandler<MarkAsReturnedBorrowingTransactionCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MarkAsReturnedBorrowingTransactionCommand command, CancellationToken ct)
    {
        var transaction = await context.BorrowingTransactions
            .FirstOrDefaultAsync(bt => bt.Id == command.BorrowingTransactionId, ct);

        if (transaction is null)
        {
            logger.LogWarning(
                "Borrowing transaction {BorrowingTransactionId} not found for marking as returned.",
                command.BorrowingTransactionId);

            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        if (transaction.BorrowerStudentId != command.StudentId)
        {
            logger.LogWarning(
                "Student {StudentId} is not the borrower for borrowing transaction {BorrowingTransactionId}.",
                command.StudentId,
                transaction.Id);

            return BorrowingTransactionApplicationErrors.StudentNotBorrower;
        }

        var now = timeProvider.GetUtcNow();
        var returnResult = transaction.ReturnBookCopy(now, now);

        if (returnResult.IsFailure)
            return returnResult.Errors;

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} has been marked as returned.",
            transaction.Id);

        return Result.Updated;
    }
}
