
namespace BookOrbit.Application.Features.Books.Queries.GetBooks;
public class GetBookQueryHandler(
    IAppDbContext context) : IRequestHandler<GetBooksQuery, Result<PaginatedList<BookListItemDto>>>
{
    public async Task<Result<PaginatedList<BookListItemDto>>> Handle(GetBooksQuery query, CancellationToken ct)
    {
        var bookQuery = context.Books.AsNoTracking();

        bookQuery = ApplyFilters(bookQuery, query);
        
        bookQuery = ApplySearchTerm(bookQuery, query);
        
        bookQuery = ApplySorting(bookQuery, query.SortColumn, query.SortDirection);

        int count = await bookQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        bookQuery = bookQuery.ApplyPagination(page, pageSize);

        var items = await bookQuery
            .Select(BookListItemDto.Projection)
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

        query = query.Where(b=>
        b.Title.Value.StartsWith(normalizedTitle) ||
        b.ISBN.Value.StartsWith(normalizedISBN) ||
        b.Publisher.Value.StartsWith(normalizedPublisher) ||
        b.Author.Value.StartsWith(normalizedAuthor));

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
            "title" => isDescending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "publisher" => isDescending ? query.OrderByDescending(b => b.Publisher) : query.OrderBy(b => b.Publisher),
            "author" => isDescending ? query.OrderByDescending(b => b.Author) : query.OrderBy(b => b.Author),
            _ => query.OrderByDescending(b => b.CreatedAtUtc)
        };
    }
}
