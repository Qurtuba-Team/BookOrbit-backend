namespace BookOrbit.Domain.Students.DomainEvents;

using BookOrbit.Domain.Common;
using BookOrbit.Domain.PointTransactions.Enums;
using System;

public class StudentPointsChangedEvent(Guid studentId, int points, PointTransactionReason reason, Guid? borrowingReviewId = null) : DomainEvent
{
    public Guid StudentId { get; } = studentId;
    public int Points { get; } = points;
    public PointTransactionReason Reason { get; } = reason;
    public Guid? BorrowingReviewId { get; } = borrowingReviewId;
}
