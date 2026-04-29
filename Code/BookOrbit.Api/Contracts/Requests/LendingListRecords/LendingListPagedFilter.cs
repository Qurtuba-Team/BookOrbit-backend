
namespace BookOrbit.Api.Contracts.Requests.LendingListRecords;
public record LendingListPagedFilter : PagedFilterRequest
{
    public Guid? BookCopyId { get; set; }
    public Guid? BookId { get;  set; }
    public List<LendingListRecordState>? States { get;  set; }
}
