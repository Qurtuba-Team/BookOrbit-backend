using BookOrbit.Application.Features.PointTransactions.Dtos;
using BookOrbit.Domain.PointTransactions.Enums;

namespace BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactions;

public record GetPointTransactionsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = "createdAt",
    string? SortDirection = "desc",
    Guid? StudentId = null,
    Guid? BorrowingReviewId = null,
    List<PointTransactionReason>? Reasons = null)
    : ICachedQuery<Result<PaginatedList<PointTransactionListItemDto>>>
{
    public string CacheKey => PointTransactionCachingConstants.PointTransactionListKey(this);

    public string[] Tags => [PointTransactionCachingConstants.PointTransactionTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(PointTransactionCachingConstants.ExpirationInMinutes);
}
