namespace BookOrbit.Domain.BorrowingRequests.DomainEvents;

using System;
using BookOrbit.Domain.Common;

public class BorrowingRequestTerminatedEvent(Guid borrowingRequestId, Guid borrowingStudentId, Guid lendingRecordId,BorrowingRequestState state) : DomainEvent
{
    public Guid BorrowingRequestId { get; } = borrowingRequestId;
    public Guid BorrowingStudentId { get; } = borrowingStudentId;
    public Guid LendingRecordId { get; } = lendingRecordId;
    public BorrowingRequestState State { get; } = state;
}
