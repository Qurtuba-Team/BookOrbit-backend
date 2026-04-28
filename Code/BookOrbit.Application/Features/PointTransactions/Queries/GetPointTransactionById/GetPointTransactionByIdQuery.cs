using BookOrbit.Application.Features.PointTransactions.Dtos;

namespace BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactionById;

public record GetPointTransactionByIdQuery(Guid PointTransactionId)
    : ICachedQuery<Result<PointTransactionDto>>
{
    public string CacheKey => PointTransactionCachingConstants.PointTransactionKey(PointTransactionId);

    public string[] Tags => [PointTransactionCachingConstants.PointTransactionTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(PointTransactionCachingConstants.ExpirationInMinutes);
}
