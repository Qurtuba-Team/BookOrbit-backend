using BookOrbit.Application.Features.BorrowingReviews.Dtos;

namespace BookOrbit.Application.Features.BorrowingReviews.Commands.CreateBorrowingReview;

public record CreateBorrowingReviewCommand(
    Guid ReviewerStudentId,
    Guid ReviewedStudentId,
    Guid BorrowingTransactionId,
    string? Description,
    int Rating) : IRequest<Result<BorrowingReviewDto>>;
