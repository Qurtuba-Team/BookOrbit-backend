
namespace BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;

public class BorrowingReviewErrors
{
    private const string ClassName = nameof(BorrowingReview);

    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    static public readonly Error RatingRequired = DomainCommonErrors.RequiredProp(ClassName, "Rating", "Rating");
    static public readonly Error ReviewerStudentIdRequired = DomainCommonErrors.RequiredProp(ClassName, "ReviewerStudentId", "Reviewer Student Id");
    static public readonly Error ReviewedStudentIdRequired = DomainCommonErrors.RequiredProp(ClassName, "ReviewedStudentId", "Reviewed Student Id");
    static public readonly Error BorrowingTransactionIdRequired = DomainCommonErrors.RequiredProp(ClassName, "BorrowingTransactionId", "Borrowing Transaction Id");
    static public readonly Error InvalidRating = DomainCommonErrors.InvalidProp(ClassName, "Rating", "Rating", $"It must be between {StarsRating.MinRating} and {StarsRating.MaxRating}");

}
