
namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEventById;

public record GetBorrowingTransactionEventByIdQuery(Guid BorrowingTransactionEventId)
    : ICachedQuery<Result<BorrowingTransactionEventDto>>
{
    public string CacheKey => BorrowingTransactionEventCachingConstants.BorrowingTransactionEventKey(BorrowingTransactionEventId);

    public string[] Tags => [BorrowingTransactionEventCachingConstants.BorrowingTransactionEventTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingTransactionEventCachingConstants.ExpirationInMinutes);
}
