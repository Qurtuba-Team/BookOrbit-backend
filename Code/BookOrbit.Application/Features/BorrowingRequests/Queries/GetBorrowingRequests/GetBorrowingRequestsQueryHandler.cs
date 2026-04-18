namespace BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequests;
public class GetBorrowingRequestsQueryHandler(
    IAppDbContext context) : IRequestHandler<GetBorrowingRequestsQuery, Result<PaginatedList<BorrowingRequestListItemDto>>>
{
    public async Task<Result<PaginatedList<BorrowingRequestListItemDto>>> Handle(GetBorrowingRequestsQuery query, CancellationToken ct)
    {
        var borrowingRequestQuery = context.BorrowingRequests.AsNoTracking();

        borrowingRequestQuery = ApplyFilters(borrowingRequestQuery, query);
        borrowingRequestQuery = ApplySearchTerm(borrowingRequestQuery, query);
        borrowingRequestQuery = ApplySorting(borrowingRequestQuery, query.SortColumn, query.SortDirection);

        int count = await borrowingRequestQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        borrowingRequestQuery = borrowingRequestQuery.ApplyPagination(page, pageSize);

        var items = await borrowingRequestQuery
            .Select(BorrowingRequestListItemDto.Projection)
            .ToListAsync(ct);

        return new PaginatedList<BorrowingRequestListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<BorrowingRequest> ApplyFilters(IQueryable<BorrowingRequest> query, GetBorrowingRequestsQuery searchQuery)
    {
        if (searchQuery.BorrowingStudentId is not null)
            query = query.Where(br => br.BorrowingStudentId == searchQuery.BorrowingStudentId);

        if (searchQuery.LendingRecordId is not null)
            query = query.Where(br => br.LendingRecordId == searchQuery.LendingRecordId);

        if (searchQuery.States is not null && searchQuery.States.Count != 0)
            query = query.Where(br => searchQuery.States.Contains(br.State));

        return query;
    }

    private static IQueryable<BorrowingRequest> ApplySearchTerm(IQueryable<BorrowingRequest> query, GetBorrowingRequestsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        var normalizedStudentName = StudentName.Normalize(searchQuery.SearchTerm);
        var normalizedBookTitle = BookTitle.Normalize(searchQuery.SearchTerm);

        return query.Where(br =>
            br.BorrowingStudent!.Name.Value.Contains(normalizedStudentName) ||
            br.LendingRecord!.BookCopy!.Book!.Title.Value.Contains(normalizedBookTitle));
    }

    private static IQueryable<BorrowingRequest> ApplySorting(IQueryable<BorrowingRequest> query, string? sortColumn, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = "createdat";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = "desc";

        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(br => br.CreatedAtUtc) : query.OrderBy(br => br.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(br => br.LastModifiedUtc) : query.OrderBy(br => br.LastModifiedUtc),
            "expirationdate" => isDescending ? query.OrderByDescending(br => br.ExpirationDateUtc) : query.OrderBy(br => br.ExpirationDateUtc),
            "state" => isDescending ? query.OrderByDescending(br => br.State) : query.OrderBy(br => br.State),
            "borrowername" => isDescending ? query.OrderByDescending(br => br.BorrowingStudent!.Name.Value) : query.OrderBy(br => br.BorrowingStudent!.Name.Value),
            "booktitle" => isDescending ? query.OrderByDescending(br => br.LendingRecord!.BookCopy!.Book!.Title.Value) : query.OrderBy(br => br.LendingRecord!.BookCopy!.Book!.Title.Value),
            _ => query.OrderByDescending(br => br.CreatedAtUtc)
        };
    }
}
