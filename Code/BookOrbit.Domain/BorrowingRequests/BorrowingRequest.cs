
namespace BookOrbit.Domain.BorrowingRequests;

public class BorrowingRequest : ExpirableEntity
{
    public Guid BorrowingStudentId { get; }
    public Guid LendingRecordId { get; }
    public BorrowingRequestState State { get; }


    public Student? BorrowingStudent { get; private set; }
    public LendingListRecord? LendingRecord { get; private set; }

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

        return borrowingRequest;
    }
}