using BookOrbit.Domain.PointTransactions.Enums;

namespace BookOrbit.Application.Features.PointTransactions.Dtos;

public class PointTransactionWithStudentNameDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid StudentId { get; set; } = Guid.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid? BorrowingReviewId { get; set; } = null;
    public int Points { get; set; }
    public PointTransactionReason Reason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastModifiedUtc { get; set; }

    public PointTransactionWithStudentNameDto() { }
}
