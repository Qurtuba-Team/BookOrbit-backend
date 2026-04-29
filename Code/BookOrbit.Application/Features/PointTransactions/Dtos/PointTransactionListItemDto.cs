
namespace BookOrbit.Application.Features.PointTransactions.Dtos;

public record PointTransactionListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid StudentId { get; set; } = Guid.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid? BorrowingReviewId { get; set; } = null;
    public int Points { get; set; }
    public PointTransactionReason Reason { get; set; }
    public PointTransactionDirection Direction { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonConstructor]
    private PointTransactionListItemDto() { }

    private PointTransactionListItemDto(
        Guid id,
        Guid studentId,
        string studentName,
        Guid? borrowingReviewId,
        int points,
        PointTransactionReason reason,
        PointTransactionDirection direction,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        StudentId = studentId;
        StudentName = studentName;
        BorrowingReviewId = borrowingReviewId;
        Points = points;
        Reason = reason;
        Direction = direction;
        CreatedAtUtc = createdAtUtc;
    }

    public static PointTransactionListItemDto FromIntermediate(PointTransactionWithStudentNameDto item)
        => new(
            item.Id,
            item.StudentId,
            item.StudentName,
            item.BorrowingReviewId,
            item.Points,
            item.Reason,
            GetDirection(item.Reason),
            item.CreatedAtUtc);

    private static bool CanDeductPoints(PointTransactionReason reason) =>
        reason is
        PointTransactionReason.BadReview or
        PointTransactionReason.Borrowing or
        PointTransactionReason.NegativeAdjustment or
        PointTransactionReason.Penalty;

    private static bool CanAddPoints(PointTransactionReason reason) =>
        reason is
        PointTransactionReason.GoodReview or
        PointTransactionReason.Returning or
        PointTransactionReason.PositiveAdjustment or
        PointTransactionReason.Reward or
        PointTransactionReason.BookBorrowedFrom;

    private static PointTransactionDirection GetDirection(PointTransactionReason reason)
    {
        var canAdd = CanAddPoints(reason);
        var canDeduct = CanDeductPoints(reason);

        if (canAdd == canDeduct)
            return PointTransactionDirection.None;

        if (canAdd)
            return PointTransactionDirection.Add;

        return PointTransactionDirection.Deduct;
    }
}
