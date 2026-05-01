namespace BookOrbit.Api.Contracts.Requests.Books;
public record CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher {  get; set; } = string.Empty;
    public List<BookCategory>? Categories { get; set; } = null;
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Optional cover image. When omitted the system automatically retrieves
    /// a cover from Open Library (primary) or Google Books (fallback).
    /// </summary>
    public IFormFile? CoverImage { get; set; } = null;
}