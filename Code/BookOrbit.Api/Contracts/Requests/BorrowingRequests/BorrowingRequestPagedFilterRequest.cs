
namespace BookOrbit.Api.Contracts.Requests.BorrowingRequests;
public record BorrowingRequestPagedFilterRequest : PagedFilterRequest
{
    public Guid? BorrowingStudentId { get; set; } = null;
    public Guid? LendingListRecordId { get; set; } = null;
    public Guid? LendingStudentId { get; set; } = null;
    public List<BorrowingRequestState>? States { get; set; } = null;
}
