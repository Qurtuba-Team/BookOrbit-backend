namespace BookOrbit.Api.Contracts.Requests.Books;
public record UpdateBookRequest
{
    public Guid BookId { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
}