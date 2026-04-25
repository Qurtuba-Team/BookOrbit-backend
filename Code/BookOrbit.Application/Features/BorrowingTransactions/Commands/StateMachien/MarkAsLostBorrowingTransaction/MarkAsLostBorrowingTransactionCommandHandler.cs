using BookOrbit.Application.Features.BorrowingTransactions;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsLostBorrowingTransaction;
public class MarkAsLostBorrowingTransactionCommandHandler(
    IAppDbContext context,
    ILogger<MarkAsLostBorrowingTransactionCommandHandler> logger)
    : IRequestHandler<MarkAsLostBorrowingTransactionCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MarkAsLostBorrowingTransactionCommand command, CancellationToken ct)
    {
        var transaction = await context.BorrowingTransactions
            .FirstOrDefaultAsync(bt => bt.Id == command.BorrowingTransactionId, ct);

        if (transaction is null)
        {
            logger.LogWarning(
                "Borrowing transaction {BorrowingTransactionId} not found for marking as lost.",
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

        var lostResult = transaction.MarkAsLost();

        if (lostResult.IsFailure)
            return lostResult.Errors;

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} has been marked as lost.",
            transaction.Id);

        return Result.Updated;
    }
}
