using BookOrbit.Application.Features.BorrowingTransactions.Dtos;

namespace BookOrbit.Application.Features.BorrowingTransactions.Queries.GetBorrowingTransactionById;
public class GetBorrowingTransactionByIdQueryHandler(
    ILogger<GetBorrowingTransactionByIdQueryHandler> logger,
    IAppDbContext context)
    : IRequestHandler<GetBorrowingTransactionByIdQuery, Result<BorrowingTransactionDto>>
{
    public async Task<Result<BorrowingTransactionDto>> Handle(GetBorrowingTransactionByIdQuery query, CancellationToken ct)
    {
        var transaction = await context.BorrowingTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(bt => bt.Id == query.BorrowingTransactionId, ct);

        if (transaction is null)
        {
            logger.LogWarning("Borrowing transaction {BorrowingTransactionId} not found.", query.BorrowingTransactionId);
            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        return BorrowingTransactionDto.FromEntity(transaction);
    }
}
