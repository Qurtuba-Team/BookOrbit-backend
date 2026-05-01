namespace BookOrbit.Domain.BorrowingTransactions.DomainEvents;

using System;
using BookOrbit.Domain.Common;
using BookOrbit.Domain.BorrowingTransactions.Enums;

public class BorrowingTransactionStateChangedEvent(Guid borrowingTransactionId, BorrowingTransactionState state) : DomainEvent
{
    public Guid BorrowingTransactionId { get; } = borrowingTransactionId;
    public BorrowingTransactionState State { get; } = state;
}
