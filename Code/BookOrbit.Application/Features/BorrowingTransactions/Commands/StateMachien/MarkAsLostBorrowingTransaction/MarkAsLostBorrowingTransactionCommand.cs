namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsLostBorrowingTransaction;

public record MarkAsLostBorrowingTransactionCommand(Guid BorrowingTransactionId, Guid StudentId) : IRequest<Result<Updated>>;
