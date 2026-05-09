
namespace BookOrbit.Application.Features.BorrowingReviews;

public static class BorrowingReviewApplicationErrors
{
    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(BorrowingReviewErrors.ClassName, "Id", "Id");
    public static readonly Error AlreadyExists = ApplicationCommonErrors.AlreadyExists(BorrowingReviewErrors.ClassName, "BorrowingTransactionId", "BorrowingTransactionId");
}
