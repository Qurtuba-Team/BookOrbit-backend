
namespace BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactions;

public record GetPointTransactionsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = QuerySettings.DefaultSortColumn,
    string? SortDirection = QuerySettings.DefaultSortDirection,
    Guid? StudentId = null,
    Guid? BorrowingReviewId = null,
    List<PointTransactionReason>? Reasons = null)
    : IPagedQuery<PointTransactionListItemDto>
{
    public string CacheKey => PointTransactionCachingConstants.PointTransactionListKey(this);

    public string[] Tags => [PointTransactionCachingConstants.PointTransactionTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(PointTransactionCachingConstants.ExpirationInMinutes);
}
