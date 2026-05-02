
namespace BookOrbit.Application.Features.BorrowingReviews.Commands.CreateBorrowingReview;

public record CreateBorrowingReviewCommand(
    Guid BorrowingTransactionId,
    string? Description,
    int Rating) : IRequest<Result<BorrowingReviewDto>>;
