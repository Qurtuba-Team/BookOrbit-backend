namespace BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecords;
public record GetLendingListRecordsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = QuerySettings.DefaultSortColumn,
    string? SortDirection = QuerySettings.DefaultSortDirection,
    Guid? BookCopyId = null,
    Guid? BookId = null,
    Guid? OwnerId = null,
    List<LendingListRecordState>? States = null)
    : IPagedQuery<LendingListRecordListItemDto>
{
    public string CacheKey => LendingListCachingConstants.LendingListListKey(this);

    public string[] Tags => [LendingListCachingConstants.LendingListTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(LendingListCachingConstants.ExpirationInMinutes);
}