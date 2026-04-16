namespace BookOrbit.Api.Contracts.Requests.BookCopies;
public record CreateBookCopyRequest
{
    public Guid BookId { get; set; } = Guid.Empty;
    public BookCopyCondition Condition { get; set; }
}