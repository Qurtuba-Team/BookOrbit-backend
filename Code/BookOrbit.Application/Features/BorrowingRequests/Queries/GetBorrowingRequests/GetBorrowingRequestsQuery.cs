namespace BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequests;
public record GetBorrowingRequestsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = "createdAt",
    string? SortDirection = "desc",
    Guid? BorrowingStudentId = null,
    Guid? LendingRecordId = null,
    List<BorrowingRequestState>? States = null)
    : ICachedQuery<Result<PaginatedList<BorrowingRequestListItemDto>>>
{
    public string CacheKey => BorrowingRequestCachingConstants.BorrowingRequestListKey(this);

    public string[] Tags => [BorrowingRequestCachingConstants.BorrowingRequestTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingRequestCachingConstants.ExpirationInMinutes);
}
