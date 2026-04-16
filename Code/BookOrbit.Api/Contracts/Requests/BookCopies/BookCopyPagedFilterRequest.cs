namespace BookOrbit.Api.Contracts.Requests.BookCopies;
public record BookCopyPagedFilterRequest : PagedFilterRequest
{
    public List<BookCopyCondition>? Conditions { get; set; } = null;
    public List<BookCopyState>? States { get; set; } = null;
}