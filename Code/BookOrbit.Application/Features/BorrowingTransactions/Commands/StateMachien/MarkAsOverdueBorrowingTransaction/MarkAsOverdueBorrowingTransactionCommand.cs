namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsOverdueBorrowingTransaction;

public record MarkAsOverdueBorrowingTransactionCommand(Guid BorrowingTransactionId) : IRequest<Result<Updated>>;
