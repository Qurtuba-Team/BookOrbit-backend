namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsReturnedBorrowingTransaction;

public record MarkAsReturnedBorrowingTransactionCommand(Guid BorrowingTransactionId, string RowVersion ) : ConcurrencyCommand<Result<Updated>>(RowVersion);
