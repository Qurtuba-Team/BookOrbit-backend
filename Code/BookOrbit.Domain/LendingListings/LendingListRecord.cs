
using BookOrbit.Domain.PointTransactions.ValueObjects;

namespace BookOrbit.Domain.LendingListings;
public class LendingListRecord : ExpirableEntity
{
    public Guid BookCopyId { get; }
    public LendingListRecordState State { get; }
    public int BorrowingDurationInDays { get; }
    public Point Cost { get; }
    
    public BookCopy? BookCopy { get; private set; }


    public const int MinCostInPoints = 1;
    public const int MaxCostInPoints = 1000;

    public const int MinBorrowingDurationInDays = 7;
    public const int MaxBorrowingDurationInDays = 30;


#pragma warning disable CS8618
    private LendingListRecord() { }

    private LendingListRecord(
        Guid id,
        Guid bookCopyId,
        int borrowingDurationInDays,
        Point cost,
        DateTimeOffset expirationDateUtc) : base(id)
    {
        BookCopyId = bookCopyId;
        State = LendingListRecordState.Available;
        BorrowingDurationInDays = borrowingDurationInDays;
        Cost = cost;
        ExpirationDateUtc = expirationDateUtc;
    }

    public static Result<LendingListRecord> Create(
        Guid id,
        Guid bookCopyId,
        int borrowingDurationInDays,
        Point cost,
        DateTimeOffset expirationDateUtc,
        DateTimeOffset currentTime // This parameter is added to make the method more testable, as it allows us to control the current time during testing. 
        )
    {
        if (id == Guid.Empty)
            return LendingListRecordErrors.IdRequired;

        if (bookCopyId == Guid.Empty)
            return LendingListRecordErrors.BookCopyIdRequired;

        if (borrowingDurationInDays > MaxBorrowingDurationInDays || borrowingDurationInDays < MinBorrowingDurationInDays)
            return LendingListRecordErrors.InvalidBorrowingDuration;

        if (cost is null)
            return LendingListRecordErrors.CostRequeired;

        if (cost.Value > MaxCostInPoints || cost.Value < MinCostInPoints)
            return LendingListRecordErrors.InvalidCostInPoints;

        if (expirationDateUtc <= currentTime)
            return LendingListRecordErrors.InvalidExpirationDate;

        return new LendingListRecord(
            id,
            bookCopyId,
            borrowingDurationInDays,
            cost,
            expirationDateUtc);
    }
}

