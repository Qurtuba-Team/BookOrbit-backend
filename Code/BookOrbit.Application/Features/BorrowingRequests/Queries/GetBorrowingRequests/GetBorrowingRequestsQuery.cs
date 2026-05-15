namespace BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequests;
public record GetBorrowingRequestsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = QuerySettings.DefaultSortColumn,
    string? SortDirection = QuerySettings.DefaultSortDirection,
    Guid? BorrowingStudentId = null,
    Guid? LendingRecordId = null,
    Guid? LendingStudentId = null,
    List<BorrowingRequestState>? States = null)
    : IPagedQuery<BorrowingRequestListItemDto>
{
    public string CacheKey => BorrowingRequestCachingConstants.BorrowingRequestListKey(this);

    public string[] Tags => [BorrowingRequestCachingConstants.BorrowingRequestTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(BorrowingRequestCachingConstants.ExpirationInMinutes);
}
