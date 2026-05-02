namespace BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecords;
public record GetLendingListRecordsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = "createdAt",
    string? SortDirection = "desc",
    Guid? BookCopyId = null,
    Guid? BookId = null,
    Guid? OwnerId = null,
    List<LendingListRecordState>? States = null)
    : ICachedQuery<Result<PaginatedList<LendingListRecordListItemDto>>>
{
    public string CacheKey => LendingListCachingConstants.LendingListListKey(this);

    public string[] Tags => [LendingListCachingConstants.LendingListTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(LendingListCachingConstants.ExpirationInMinutes);
}