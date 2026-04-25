namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsReturnedBorrowingTransaction;

public record MarkAsReturnedBorrowingTransactionCommand(Guid BorrowingTransactionId, Guid StudentId) : IRequest<Result<Updated>>;
