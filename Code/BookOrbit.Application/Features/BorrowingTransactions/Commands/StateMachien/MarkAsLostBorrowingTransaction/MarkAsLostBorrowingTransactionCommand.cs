namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsLostBorrowingTransaction;

public record MarkAsLostBorrowingTransactionCommand(Guid BorrowingTransactionId, string RowVersion) : ConcurrencyCommand<Result<Updated>>(RowVersion);
