using BookOrbit.Application.Features.BorrowingTransactions.Dtos;

namespace BookOrbit.Application.Features.BorrowingTransactions.Queries.GetBorrowingTransactions;
public record GetBorrowingTransactionsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = "createdAt",
    string? SortDirection = "desc",
    Guid? BorrowerStudentId = null,
    Guid? LenderStudentId = null,
    Guid? BookCopyId = null,
    Guid? BorrowingRequestId = null,
    List<BorrowingTransactionState>? States = null)
    : ICachedQuery<Result<PaginatedList<BorrowingTransactionListItemDto>>>
{
    public string CacheKey => BorrowingTransactionCachingConstants.BorrowingTransactionListKey(this);

    public string[] Tags => [BorrowingTransactionCachingConstants.BorrowingTransactionTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingTransactionCachingConstants.ExpirationInMinutes);
}
