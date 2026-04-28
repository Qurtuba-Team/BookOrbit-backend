using BookOrbit.Application.Features.BorrowingTransactionEvents.Dtos;
using BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;

namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEventById;

public class GetBorrowingTransactionEventByIdQueryHandler(
    ILogger<GetBorrowingTransactionEventByIdQueryHandler> logger,
    IAppDbContext context)
    : IRequestHandler<GetBorrowingTransactionEventByIdQuery, Result<BorrowingTransactionEventDto>>
{
    public async Task<Result<BorrowingTransactionEventDto>> Handle(GetBorrowingTransactionEventByIdQuery query, CancellationToken ct)
    {
        var transactionEvent = await context.BorrowingTransactionEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(bte => bte.Id == query.BorrowingTransactionEventId, ct);

        if (transactionEvent is null)
        {
            logger.LogWarning("Borrowing transaction event {BorrowingTransactionEventId} not found.", query.BorrowingTransactionEventId);
            return BorrowingTransactionEventApplicationErrors.NotFoundById;
        }

        return BorrowingTransactionEventDto.FromEntity(transactionEvent);
    }
}
