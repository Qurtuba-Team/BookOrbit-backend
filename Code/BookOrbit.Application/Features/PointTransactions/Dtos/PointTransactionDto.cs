using BookOrbit.Domain.PointTransactions;
using BookOrbit.Domain.PointTransactions.Enums;

namespace BookOrbit.Application.Features.PointTransactions.Dtos;

public record PointTransactionDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid StudentId { get; set; } = Guid.Empty;
    public Guid? BorrowingReviewId { get; set; } = null;
    public int Points { get; set; }
    public PointTransactionReason Reason { get; set; }
    public PointTransactionDirection Direction { get; set; }

    [JsonConstructor]
    private PointTransactionDto() { }

    private PointTransactionDto(
        Guid id,
        Guid studentId,
        Guid? borrowingReviewId,
        int points,
        PointTransactionReason reason,
        PointTransactionDirection direction)
    {
        Id = id;
        StudentId = studentId;
        BorrowingReviewId = borrowingReviewId;
        Points = points;
        Reason = reason;
        Direction = direction;
    }

    public static PointTransactionDto FromEntity(PointTransaction transaction)
        => new(
            transaction.Id,
            transaction.StudentId,
            transaction.BorrowingReviewId,
            transaction.Points,
            transaction.Reason,
            transaction.Direction);
}
