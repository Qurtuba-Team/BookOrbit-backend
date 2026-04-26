using BookOrbit.Application.Features.BorrowingTransactions.Dtos;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.CreateBorrowingTransaction;

public record CreateBorrowingTransactionCommand(Guid BorrowingRequestId) : IRequest<Result<BorrowingTransactionDto>>;
