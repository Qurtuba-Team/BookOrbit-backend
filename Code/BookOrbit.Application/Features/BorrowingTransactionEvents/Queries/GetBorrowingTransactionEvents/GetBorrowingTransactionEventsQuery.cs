
namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEvents;

public record GetBorrowingTransactionEventsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = QuerySettings.DefaultSortColumn,
    string? SortDirection = QuerySettings.DefaultSortDirection,
    Guid? BorrowingTransactionId = null,
    List<BorrowingTransactionState>? States = null)
    : IPagedQuery<BorrowingTransactionEventListItemDto>
{
    public string CacheKey => BorrowingTransactionEventCachingConstants.BorrowingTransactionEventListKey(this);

    public string[] Tags => [BorrowingTransactionEventCachingConstants.BorrowingTransactionEventTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingTransactionEventCachingConstants.ExpirationInMinutes);
}
