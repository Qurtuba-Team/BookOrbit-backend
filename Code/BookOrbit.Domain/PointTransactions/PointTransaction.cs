
namespace BookOrbit.Domain.PointTransactions;

public class PointTransaction : AuditableEntity
{
    public Guid StudentId { get; }
    public Guid? BorrowingReviewId { get; }
    public int Points { get; }
    public PointTransactionReason Reason { get; }

    public PointTransactionDirection Direction => GetDirection(Reason);

    private PointTransaction() { }
    private PointTransaction(
        Guid id,
        Guid studentId,
        Guid? borrowingReviewId,
        int points,
        PointTransactionReason reason) : base(id)
    {
        StudentId = studentId;
        BorrowingReviewId = borrowingReviewId;
        Points = points;
        Reason = reason;
    }

    static private bool CanDeductPoints(PointTransactionReason reason) =>
        reason is
        PointTransactionReason.BadReview or
        PointTransactionReason.Borrowing or
        PointTransactionReason.NegativeAdjustment or
        PointTransactionReason.Penalty;
    static private bool CanAddPoints(PointTransactionReason reason) =>
        reason is
        PointTransactionReason.GoodReview or
        PointTransactionReason.Returning or
        PointTransactionReason.PositiveAdjustment or
        PointTransactionReason.Reward or
        PointTransactionReason.BookBorrowedFrom;
    static private PointTransactionDirection GetDirection(PointTransactionReason reason)
    {
        var canAdd = CanAddPoints(reason);
        var canDeduct = CanDeductPoints(reason);

        //Guard against invalid reasons that are not categorized as either add or deduct
        if (canAdd == canDeduct)
            return PointTransactionDirection.None;

        if (canAdd)
            return PointTransactionDirection.Add;
        if (canDeduct)
            return PointTransactionDirection.Deduct;

        return PointTransactionDirection.None;
    }


    static public Result<PointTransaction> Create(
        Guid id,
        Guid studentId,
        Guid? borrowingReviewId,
        int points,
        PointTransactionReason reason)
    {
        if (id == Guid.Empty)
            return PointTransactionErrors.IdRequired;

        if (studentId == Guid.Empty)
            return PointTransactionErrors.StudentIdRequired;

        if (borrowingReviewId.HasValue && borrowingReviewId.Value == Guid.Empty)
            return PointTransactionErrors.InvalidBorrowingReviewId;

        if (points <= 0)
            return PointTransactionErrors.InvalidPoints;

        if (!Enum.IsDefined(reason))
            return PointTransactionErrors.InvalidReason;

        bool isReviewReason =
            reason is
            PointTransactionReason.BadReview or
            PointTransactionReason.GoodReview;

        if (isReviewReason && borrowingReviewId is null)
            return PointTransactionErrors.ReviewReasonShouldHaveBorrowingReviewId;

        if (!isReviewReason && borrowingReviewId is not null)
            return PointTransactionErrors.NonReviewReasonShouldNotHaveBorrowingReviewId;

        var direction = GetDirection(reason);

        if(direction is PointTransactionDirection.None)
            return PointTransactionErrors.InvalidPointTransactionReason;


        return new PointTransaction(
            id,
            studentId,
            borrowingReviewId,
            points,
            reason);
    }
}