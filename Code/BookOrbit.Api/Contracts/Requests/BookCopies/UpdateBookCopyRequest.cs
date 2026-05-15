namespace BookOrbit.Api.Contracts.Requests.BookCopies;
public class UpdateBookCopyRequest
{
    public BookCopyCondition Condition { get; set; }
    public string? RowVersion { get; set; }
}