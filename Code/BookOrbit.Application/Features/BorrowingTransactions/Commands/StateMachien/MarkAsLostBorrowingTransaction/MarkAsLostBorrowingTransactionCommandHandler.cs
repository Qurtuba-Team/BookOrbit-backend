using BookOrbit.Application.Features.BorrowingTransactions;
using BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;

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

        context.BorrowingTransactionEvents.Add(logCreationResult.Value);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} has been marked as lost.",
            transactionData.BorrowingTransaction.Id);

        return Result.Updated;
    }
}
