namespace BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecords;
public class GetLendingListRecordsQueryHandler(
    IAppDbContext context) : IRequestHandler<GetLendingListRecordsQuery, Result<PaginatedList<LendingListRecordListItemDto>>>
{
    public async Task<Result<PaginatedList<LendingListRecordListItemDto>>> Handle(GetLendingListRecordsQuery query, CancellationToken ct)
    {
        var lendingListQuery = context.LendingListRecords.AsNoTracking();

        lendingListQuery = ApplyFilters(lendingListQuery, query);

        lendingListQuery = ApplySearchTerm(lendingListQuery, query);

        lendingListQuery = ApplySorting(lendingListQuery, query.SortColumn, query.SortDirection);

        int count = await lendingListQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        lendingListQuery = lendingListQuery.ApplyPagination(page, pageSize);

        var items = await lendingListQuery
            .Select(LendingListRecordListItemDto.Projection)
            .ToListAsync(ct);

        return new PaginatedList<LendingListRecordListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<LendingListRecord> ApplyFilters(IQueryable<LendingListRecord> query, GetLendingListRecordsQuery searchQuery)
    {
        if (searchQuery.BookCopyId is not null)
            query = query.Where(lr => lr.BookCopyId == searchQuery.BookCopyId);

        if (searchQuery.BookId is not null)
            query = query.Where(lr => lr.BookCopy!.BookId == searchQuery.BookId);

        if (searchQuery.States is not null &&
            searchQuery.States.Count != 0)
            query = query.Where(lr => searchQuery.States.Contains(lr.State));

        return query;
    }

    private static IQueryable<LendingListRecord> ApplySearchTerm(IQueryable<LendingListRecord> query, GetLendingListRecordsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;
        
        var normalizedTitle = BookTitle.Normalize(searchQuery.SearchTerm);

        query = query.Where(lr => lr.BookCopy!.Book!.Title.Value.Contains(normalizedTitle));

        return query;
    }

    private static IQueryable<LendingListRecord> ApplySorting(IQueryable<LendingListRecord> query, string? sortColumn, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = "createdat";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = "desc";

        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(lr => lr.CreatedAtUtc) : query.OrderBy(lr => lr.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(lr => lr.LastModifiedUtc) : query.OrderBy(lr => lr.LastModifiedUtc),
            "cost" => isDescending ? query.OrderByDescending(lr => lr.Cost.Value) : query.OrderBy(lr => lr.Cost.Value),
            "borrowingduration" => isDescending ? query.OrderByDescending(lr => lr.BorrowingDurationInDays) : query.OrderBy(lr => lr.BorrowingDurationInDays),
            "expirationdate" => isDescending ? query.OrderByDescending(lr => lr.ExpirationDateUtc) : query.OrderBy(lr => lr.ExpirationDateUtc),
            "state" => isDescending ? query.OrderByDescending(lr => lr.State) : query.OrderBy(lr => lr.State),
            _ => query.OrderByDescending(lr => lr.CreatedAtUtc)
        };
    }
}
