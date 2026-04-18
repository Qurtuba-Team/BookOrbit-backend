namespace BookOrbit.Api.Contracts.Requests.BookCopies;
public record CreateBookCopyRequest
{
    public BookCopyCondition Condition { get; set; }
}