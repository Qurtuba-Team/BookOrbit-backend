namespace BookOrbit.Api.Contracts.Requests.Books;
public record BookPagedFilterRequest : PagedFilterRequest
{
    public List<BookCategory>? Categories { get; set; } = null;
}