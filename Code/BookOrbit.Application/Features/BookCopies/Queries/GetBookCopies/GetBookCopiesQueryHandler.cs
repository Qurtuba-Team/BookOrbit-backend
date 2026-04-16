
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;
using BookOrbit.Domain.PointTransactions.ValueObjects;

namespace BookOrbit.Application.Features.BookCopies.Queries.GetBookCopies;
public class GetBookCopiesQueryHandler(
    IAppDbContext context) : IRequestHandler<GetBookCopiesQuery, Result<PaginatedList<BookCopyListItemDto>>>
{
    public async Task<Result<PaginatedList<BookCopyListItemDto>>> Handle(GetBookCopiesQuery query, CancellationToken ct)
    {
        var bookCopyQuery = context.BookCopies.AsNoTracking();

        bookCopyQuery = ApplyFilters(bookCopyQuery, query);

        bookCopyQuery = ApplySearchTerm(bookCopyQuery, query);

        bookCopyQuery = ApplySorting(bookCopyQuery, query.SortColumn, query.SortDirection);

        int count = await bookCopyQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        bookCopyQuery = bookCopyQuery.ApplyPagination(page, pageSize);

        var items = await bookCopyQuery
            .Select(BookCopyListItemDto.Projection)
            .ToListAsync(ct);

        return new PaginatedList<BookCopyListItemDto>
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
        (b.Book!.Title.Value.StartsWith(normalizedTitle)) ||
        (b.Owner!.Name.Value.StartsWith(normalizedStudentName)));

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
