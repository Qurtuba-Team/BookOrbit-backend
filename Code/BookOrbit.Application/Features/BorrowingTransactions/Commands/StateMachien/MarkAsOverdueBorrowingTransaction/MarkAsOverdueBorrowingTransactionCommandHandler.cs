using BookOrbit.Application.Features.BorrowingTransactions;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsOverdueBorrowingTransaction;
public class MarkAsOverdueBorrowingTransactionCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider,
    ILogger<MarkAsOverdueBorrowingTransactionCommandHandler> logger)
    : IRequestHandler<MarkAsOverdueBorrowingTransactionCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MarkAsOverdueBorrowingTransactionCommand command, CancellationToken ct)
    {
        var transaction = await context.BorrowingTransactions
            .FirstOrDefaultAsync(bt => bt.Id == command.BorrowingTransactionId, ct);

        if (transaction is null)
        {
            logger.LogWarning(
                "Borrowing transaction {BorrowingTransactionId} not found for marking as overdue.",
                command.BorrowingTransactionId);

            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        var now = timeProvider.GetUtcNow();
        var overdueResult = transaction.MarkAsOverdue(now);

        if (overdueResult.IsFailure)
            return overdueResult.Errors;

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Borrowing transaction {BorrowingTransactionId} has been marked as overdue.",
            transaction.Id);

        return Result.Updated;
    }
}
