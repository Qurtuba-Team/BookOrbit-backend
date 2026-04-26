namespace BookOrbit.Domain.BorrowingRequests.DomainEvents;
public class BorrowingRequestAcceptedEvent(Guid BorrowingStudentId,Guid BorrowingRequestId) : DomainEvent
{
    public Guid BorrowingStudentId { get; set; } = BorrowingStudentId;
    public Guid BorrowingRequestId { get; set; } = BorrowingRequestId;
}