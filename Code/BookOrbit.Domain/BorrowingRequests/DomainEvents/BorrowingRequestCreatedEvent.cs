namespace BookOrbit.Domain.BorrowingRequests.DomainEvents;
public class BorrowingRequestCreatedEvent(Guid BorrowingRequestId, Guid LendingRecordId) : DomainEvent
{
    public Guid BorrowingRequestId { get; } = BorrowingRequestId;
    public Guid LendingRecordId { get; } = LendingRecordId;
}
