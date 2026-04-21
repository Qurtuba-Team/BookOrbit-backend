namespace BookOrbit.Application.Features.BookCopies.Queries.GetBookCopies;
public class GetBookCopiesQueryHandler(
    IAppDbContext context,
    IRouteService routeService) : IRequestHandler<GetBookCopiesQuery, Result<PaginatedList<BookCopyListItemDto>>>
{
    public async Task<Result<PaginatedList<BookCopyListItemDto>>> Handle(GetBookCopiesQuery query, CancellationToken ct)
    {
        var bookCopyQuery = context.BookCopies.AsNoTracking();

        bookCopyQuery = ApplyFilters(bookCopyQuery, query);
        bookCopyQuery = ApplySearchTerm(bookCopyQuery, query);
        bookCopyQuery = ApplySorting(bookCopyQuery, query.SortColumn, query.SortDirection);

        var queryWithIsListed = bookCopyQuery
            .Select(b => new BookCopyWithIsListedDto
            {
                Id = b.Id,
                BookId = b.BookId,
                OwnerId = b.OwnerId,
                Condition = b.Condition,
                State = b.State,
                OwnerName = b.Owner!.Name.Value,
                Title = b.Book!.Title.Value,
                IsListed = context.LendingListRecords.Any(l => l.BookCopyId == b.Id &&
                (l.State == LendingListRecordState.Available
                ||
                l.State == LendingListRecordState.Reserved
                ||
                l.State == LendingListRecordState.Borrowed)),
                BookCoverImageFileName = b.Book.CoverImageFileName
            });

        int count = await queryWithIsListed.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        string baseUrl = routeService.GetBookCoverImageRoute();

        var items = await queryWithIsListed
            .ApplyPagination(page, pageSize)
            .Select(s => new BookCopyListItemDto(
                s.Id,
                s.BookId,
                s.OwnerId,
                s.Condition,
                s.State,
                s.OwnerName,
                s.Title,
                s.IsListed,
                (baseUrl + "/" + s.BookCoverImageFileName)
            ))

            .ToListAsync(ct); return new PaginatedList<BookCopyListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<BookCopy> ApplySorting(IQueryable<BookCopy> query, string? sortColumn, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = "createdat";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = "desc";

        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(b => b.CreatedAtUtc) : query.OrderBy(b => b.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(b => b.LastModifiedUtc) : query.OrderBy(b => b.LastModifiedUtc),
            "booktitle" => isDescending ? query.OrderByDescending(b => b.Book!.Title.Value) : query.OrderBy(b => b.Book!.Title.Value),
            "ownername" => isDescending ? query.OrderByDescending(b => b.Owner!.Name.Value) : query.OrderBy(b => b.Owner!.Name.Value),
            _ => query.OrderByDescending(b => b.CreatedAtUtc)
        };
    }

    private static IQueryable<BookCopy> ApplySearchTerm(IQueryable<BookCopy> query, GetBookCopiesQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;//no need for filters

        var normalizedTitle = BookTitle.Normalize(searchQuery.SearchTerm);
        var normalizedStudentName = StudentName.Normalize(searchQuery.SearchTerm);

        query = query.Where(b =>
        (b.Book!.Title.Value.Contains(normalizedTitle)) ||
        (b.Owner!.Name.Value.Contains(normalizedStudentName)));

        return query;
    }

    private static IQueryable<BookCopy> ApplyFilters(IQueryable<BookCopy> query, GetBookCopiesQuery searchQuery)
    {
        if(searchQuery.OwnerId is not null)
            query = query.Where(b => b.OwnerId == searchQuery.OwnerId);

        if(searchQuery.BookId is not null)
            query = query.Where(b=> b.BookId == searchQuery.BookId);

        if (searchQuery.States is not null &&
            searchQuery.States.Count != 0)
            query = query.Where(b => searchQuery.States.Contains(b.State));

        if (searchQuery.Conditions is not null &&
            searchQuery.Conditions.Count != 0)
            query = query.Where(b => searchQuery.Conditions.Contains(b.Condition));

        return query;
    }
}
