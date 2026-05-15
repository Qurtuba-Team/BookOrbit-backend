
namespace BookOrbit.Application.Features.Books.Queries.GetBooks;
public class GetBooksQueryHandler(
    IAppDbContext context,
    IRouteService routeService) : BasePagedQueryHandler<Book, BookListItemDto, GetBooksQuery>
{
    protected override Dictionary<string, Func<IQueryable<Book>, bool, IOrderedQueryable<Book>>> SortMappings
        => new()
        {
            ["createdat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.CreatedAtUtc)
                    : query.OrderBy(b => b.CreatedAtUtc),
            ["updatedat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.LastModifiedUtc)
                    : query.OrderBy(b => b.LastModifiedUtc),
            ["title"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.Title.Value)
                    : query.OrderBy(b => b.Title.Value),
            ["publisher"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.Publisher.Value)
                    : query.OrderBy(b => b.Publisher.Value),
            ["author"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.Author.Value)
                    : query.OrderBy(b => b.Author.Value)
        };

    protected override IQueryable<Book> GetBaseQuery()
    {
        return context.Books.AsNoTracking();
    }

    protected override IQueryable<Book> ApplySearch(IQueryable<Book> query, GetBooksQuery searchQuery)
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

    protected override IQueryable<Book> ApplyFilters(IQueryable<Book> query, GetBooksQuery searchQuery)
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

    protected override IQueryable<BookListItemDto> ProjectToDto(IQueryable<Book> query)
    {
        string baseUrl = routeService.GetBookCoverImageRoute();

        return query.Select(b => new BookListItemDto(
            b.Id,
            b.Title.Value,
            b.ISBN.Value,
            b.Publisher.Value,
            b.Category,
            b.Author.Value,
            context.BookCopies.Count(c => c.BookId == b.Id && c.State == BookCopyState.Available),
            baseUrl + "/" + b.CoverImageFileName,
            b.Status,
            b.RowVersion));
    }
}
