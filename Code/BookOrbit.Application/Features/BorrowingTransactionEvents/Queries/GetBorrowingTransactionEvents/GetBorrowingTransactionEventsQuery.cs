using BookOrbit.Application.Features.BorrowingTransactionEvents.Dtos;

namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEvents;

public record GetBorrowingTransactionEventsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = "createdAt",
    string? SortDirection = "desc",
    Guid? BorrowingTransactionId = null,
    List<BorrowingTransactionState>? States = null)
    : ICachedQuery<Result<PaginatedList<BorrowingTransactionEventListItemDto>>>
{
    public string CacheKey => BorrowingTransactionEventCachingConstants.BorrowingTransactionEventListKey(this);

    public string[] Tags => [BorrowingTransactionEventCachingConstants.BorrowingTransactionEventTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingTransactionEventCachingConstants.ExpirationInMinutes);
}
