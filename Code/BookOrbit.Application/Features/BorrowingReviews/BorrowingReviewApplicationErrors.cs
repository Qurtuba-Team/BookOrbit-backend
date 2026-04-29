
namespace BookOrbit.Application.Features.BorrowingReviews;

public static class BorrowingReviewApplicationErrors
{
    private const string ClassName = nameof(BorrowingReview);

    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
    public static readonly Error AlreadyExists = ApplicationCommonErrors.AlreadyExists(ClassName, "BorrowingTransactionId", "BorrowingTransactionId");
}
