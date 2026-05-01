
using BookOrbit.Domain.BorrowingRequests.DomainEvents;

namespace BookOrbit.Domain.BorrowingRequests;

public class BorrowingRequest : ExpirableEntity
{
    public Guid BorrowingStudentId { get; }
    public Guid LendingRecordId { get; }
    public BorrowingRequestState State { get; private set; }


    public Student? BorrowingStudent { get; private set; }
    public LendingListRecord? LendingRecord { get; private set; }

    public const int DefaultExpirationDays = 14;

    private BorrowingRequest(){ }
    private BorrowingRequest(
        Guid id, 
        Guid borrowingStudentId, 
        Guid lendingRecordId,
        DateTimeOffset expirationDateUtc) : base(id)
    {
        BorrowingStudentId = borrowingStudentId;
        LendingRecordId = lendingRecordId;
        State = BorrowingRequestState.Pending;
        ExpirationDateUtc = expirationDateUtc;
    }

    static public Result<BorrowingRequest> Create(
        Guid id,
        Guid borrowingStudentId,
        Guid lendingRecordId,
        DateTimeOffset expirationDateUtc,
        DateTimeOffset currentTime // This parameter is added to make the method more testable, as it allows us to control the current time during testing. 
        )
    {
        if (id == Guid.Empty)
            return BorrowingRequestErrors.IdRequired;

        if (borrowingStudentId == Guid.Empty)
            return BorrowingRequestErrors.BorrowingStudentIdRequired;

        if (lendingRecordId == Guid.Empty)
            return BorrowingRequestErrors.LendingRecordIdRequired;
       
        if (expirationDateUtc <= currentTime)
            return BorrowingRequestErrors.InvalidExpirationDate;
       
        var borrowingRequest = new BorrowingRequest(
            id,
            borrowingStudentId,
            lendingRecordId,
            expirationDateUtc);

        borrowingRequest.AddDomainEvent(new BookOrbit.Domain.BorrowingRequests.DomainEvents.BorrowingRequestCreatedEvent(borrowingRequest.Id, borrowingRequest.LendingRecordId));

        return borrowingRequest;
    }

    private bool CanTransitionToState(BorrowingRequestState newState)
    {
        return State switch
        {
            BorrowingRequestState.Pending => newState is BorrowingRequestState.Accepted or BorrowingRequestState.Rejected or BorrowingRequestState.Cancelled or BorrowingRequestState.Expired,
            BorrowingRequestState.Accepted => newState is BorrowingRequestState.Cancelled or BorrowingRequestState.Expired,
            BorrowingRequestState.Rejected => false,
            BorrowingRequestState.Cancelled => false,
            BorrowingRequestState.Expired => false,
            _ => false
        };
    }

    private Result<Updated> UpdateState(BorrowingRequestState newState)
    {
        if (!CanTransitionToState(newState))
            return BorrowingRequestErrors.InvalidStateTransition(State, newState);

        State = newState;
        return Result.Updated;
    }

    public Result<Updated> MarkAsApproved()
    {
        var result = UpdateState(BorrowingRequestState.Accepted);
        if (result.IsSuccess)
        {
            AddDomainEvent(new BookOrbit.Domain.BorrowingRequests.DomainEvents.BorrowingRequestAcceptedEvent(BorrowingStudentId, Id));
        }
        return result;
    }

    public Result<Updated> MarkAsRejected()
    {
        var result = UpdateState(BorrowingRequestState.Rejected);
        if (result.IsSuccess)
        {
            AddDomainEvent(new BorrowingRequestTerminatedEvent(Id, BorrowingStudentId, LendingRecordId));
        }
        return result;
    }

    public Result<Updated> MarkAsCancelled()
    {
        var result = UpdateState(BorrowingRequestState.Cancelled);
        if (result.IsSuccess)
        {
            AddDomainEvent(new BorrowingRequestTerminatedEvent(Id, BorrowingStudentId, LendingRecordId));
        }
        return result;
    }

    public Result<Updated> MarkAsExpired()
    {
        var result = UpdateState(BorrowingRequestState.Expired);
        if (result.IsSuccess)
        {
            AddDomainEvent(new BorrowingRequestTerminatedEvent(Id, BorrowingStudentId, LendingRecordId));
        }
        return result;
    }
}