using BookOrbit.Application.Features.PointTransactions.Dtos;
using BookOrbit.Domain.PointTransactions;

namespace BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactionById;

public class GetPointTransactionByIdQueryHandler(
    ILogger<GetPointTransactionByIdQueryHandler> logger,
    IAppDbContext context)
    : IRequestHandler<GetPointTransactionByIdQuery, Result<PointTransactionDto>>
{
    public async Task<Result<PointTransactionDto>> Handle(GetPointTransactionByIdQuery query, CancellationToken ct)
    {
        var transaction = await context.PointTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.Id == query.PointTransactionId, ct);

        if (transaction is null)
        {
            logger.LogWarning("Point transaction {PointTransactionId} not found.", query.PointTransactionId);
            return PointTransactionApplicationErrors.NotFoundById;
        }

        return PointTransactionDto.FromEntity(transaction);
    }
}
