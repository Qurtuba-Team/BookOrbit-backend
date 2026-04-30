namespace BookOrbit.Api.Contracts.Requests.Books;
public record CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher {  get; set; } = string.Empty;
    public List<BookCategory>? Categories { get; set; } = null;
    public string Author { get; set; } = string.Empty;
    public IFormFile CoverImage { get; set; } = default!;
}