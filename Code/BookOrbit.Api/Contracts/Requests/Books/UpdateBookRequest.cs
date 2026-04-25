namespace BookOrbit.Api.Contracts.Requests.Books;
public record UpdateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public IFormFile? CoverImage { get; set; } = null;
}