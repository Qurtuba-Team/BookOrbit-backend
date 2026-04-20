namespace BookOrbit.Application.Features.BorrowingRequests.Dtos;
public record BorrowingRequestDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BorrowingStudentId { get; set; } = Guid.Empty;
    public Guid LendingRecordId { get; set; } = Guid.Empty;
    public BorrowingRequestState State { get; set; }
    public DateTimeOffset? ExpirationDateUtc { get; set; }

    [JsonConstructor]
    private BorrowingRequestDto() { }

    private BorrowingRequestDto(
        Guid id,
        Guid borrowingStudentId,
        Guid lendingRecordId,
        BorrowingRequestState state,
        DateTimeOffset? expirationDateUtc)
    {
        Id = id;
        BorrowingStudentId = borrowingStudentId;
        LendingRecordId = lendingRecordId;
        State = state;
        ExpirationDateUtc = expirationDateUtc;
    }

    public static BorrowingRequestDto FromEntity(BorrowingRequest borrowingRequest)
    {
        return new BorrowingRequestDto(
            borrowingRequest.Id,
            borrowingRequest.BorrowingStudentId,
            borrowingRequest.LendingRecordId,
            borrowingRequest.State,
            borrowingRequest.ExpirationDateUtc);
    }
}
