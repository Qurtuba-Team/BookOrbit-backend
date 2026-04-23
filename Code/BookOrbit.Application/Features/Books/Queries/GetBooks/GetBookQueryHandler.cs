
namespace BookOrbit.Application.Features.Books.Queries.GetBooks;
public class GetBookQueryHandler(
    IAppDbContext context,
    IRouteService routeService) : IRequestHandler<GetBooksQuery, Result<PaginatedList<BookListItemDto>>>
    
{
    public async Task<Result<PaginatedList<BookListItemDto>>> Handle(GetBooksQuery query, CancellationToken ct)
    {
        var bookQuery = context.Books.AsNoTracking();

        bookQuery = ApplyFilters(bookQuery, query);

        bookQuery = ApplySearchTerm(bookQuery, query);

        bookQuery = ApplySorting(bookQuery, query.SortColumn, query.SortDirection);

        var bookQueryWithCount = bookQuery
    .Select(b => new BookWithCountDto
    {
        Id = b.Id,
        Title = b.Title.Value,
        ISBN = b.ISBN.Value,
        Publisher = b.Publisher.Value,
        Category = b.Category,
        Author = b.Author.Value,
        AvailableCopiesCount = context.BookCopies
            .Where(c => c.BookId == b.Id && c.State == BookCopyState.Available)
            .Count(),
        BookCoverImageFileName = b.CoverImageFileName,
        Status = b.Status
    });

        int count = await bookQueryWithCount.CountAsync(ct);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);

        string baseUrl = routeService.GetBookCoverImageRoute();

        var items = await bookQueryWithCount
            .ApplyPagination(page, pageSize)
            .Select(s => new BookListItemDto(
                s.Id,
                s.Title,
                s.ISBN,
                s.Publisher,
                s.Category,
                s.Author,
                s.AvailableCopiesCount,
                baseUrl + "/" + s.BookCoverImageFileName,
                s.Status))
            .ToListAsync(ct);

        return new PaginatedList<BookListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };

    }

    private static IQueryable<Book> ApplySearchTerm(IQueryable<Book> query, GetBooksQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;//no need for filters

        var normalizedTitle = BookTitle.Normalize(searchQuery.SearchTerm);
        var normalizedISBN = ISBN.Normalize(searchQuery.SearchTerm);
        var normalizedPublisher = BookPublisher.Normalize(searchQuery.SearchTerm);
        var normalizedAuthor = BookAuthor.Normalize(searchQuery.SearchTerm);

        query = query.Where(b =>
        b.Title.Value.Contains(normalizedTitle) ||
        b.ISBN.Value.Contains(normalizedISBN) ||
        b.Publisher.Value.Contains(normalizedPublisher) ||
        b.Author.Value.Contains(normalizedAuthor));

        return query;
    }

    private static IQueryable<Book> ApplyFilters(IQueryable<Book> query, GetBooksQuery searchQuery)
    {
        if (searchQuery.Categories is not null &&
            searchQuery.Categories.Count != 0)
        {
            BookCategory? category = 0;

            foreach (var cat in searchQuery.Categories)
            {
                category |= cat;
            }

            query = query.Where(b => (b.Category & category) == category);
        }

        if(searchQuery.Statuses is not null &&
            searchQuery.Statuses.Count != 0)
        {
           query = query.Where(b => searchQuery.Statuses.Contains(b.Status));
        }

        return query;
    }

    private static IQueryable<Book> ApplySorting(IQueryable<Book> query, string? sortColumn, string? sortDirection)
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
            "title" => isDescending ? query.OrderByDescending(b => b.Title.Value) : query.OrderBy(b => b.Title.Value),
            "publisher" => isDescending ? query.OrderByDescending(b => b.Publisher.Value) : query.OrderBy(b => b.Publisher.Value),
            "author" => isDescending ? query.OrderByDescending(b => b.Author.Value) : query.OrderBy(b => b.Author.Value),
            _ => query.OrderByDescending(b => b.CreatedAtUtc)
        };
    }
}
