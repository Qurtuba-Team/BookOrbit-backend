using BookOrbit.Application.Features.BorrowingTransactions.Dtos;

namespace BookOrbit.Application.Features.BorrowingTransactions.Queries.GetBorrowingTransactionById;
public record GetBorrowingTransactionByIdQuery(Guid BorrowingTransactionId, Guid StudentId)
    : ICachedQuery<Result<BorrowingTransactionDto>>
{
    public string CacheKey => BorrowingTransactionCachingConstants.BorrowingTransactionKey(BorrowingTransactionId);

    public string[] Tags => [BorrowingTransactionCachingConstants.BorrowingTransactionTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingTransactionCachingConstants.ExpirationInMinutes);
}
