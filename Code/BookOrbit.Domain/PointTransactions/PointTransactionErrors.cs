namespace BookOrbit.Domain.PointTransactions;
static public class PointTransactionErrors
{
    private const string ClassName = nameof(PointTransaction);

    public static readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    public static readonly Error StudentIdRequired = DomainCommonErrors.RequiredProp(ClassName, "StudentId", "Student Id");
    public static readonly Error InvalidPoints = DomainCommonErrors.CustomValidation(ClassName, "PointsCannotBeZero", "Points should be more than 0.");
    public static readonly Error InvalidBorrowingReviewId = DomainCommonErrors.CustomValidation(ClassName, "InvalidBorrowingReviewId", "BorrowingReviewId cannot be empty Guid.");
    public static readonly Error InvalidPointTransactionReason = DomainCommonErrors.CustomValidation(ClassName, "InvalidPointTransactionReason", "The reason provided is not a valid Point Transaction Reason.");
    public static readonly Error ReasonIsNotConsistnantWithDiriction = DomainCommonErrors.CustomValidation(ClassName, "ReasonIsNotConsisttantWithDiriction", "The reason provided is not consistent with the direction of the point transaction.");
    public static readonly Error ReviewReasonShouldHaveBorrowingReviewId = DomainCommonErrors.CustomValidation(ClassName, "ReviewReasonShouldHaveBorrowingReviewId", "When the reason is Good Review or Bad Review, Borrowing Review Id must be provided.");
    public static readonly Error NonReviewReasonShouldNotHaveBorrowingReviewId = DomainCommonErrors.CustomValidation(ClassName, "NonReviewReasonShouldNotHaveBorrowingReviewId", "When the reason is not Good Review or Bad Review, Borrowing Review Id should not be provided.");
    public static readonly Error InvalidDeductionReason = DomainCommonErrors.CustomValidation(ClassName, "InvalidDeductionReason", "The reason provided is not valid for deducting points.");
    public static readonly Error InvalidAddingReason = DomainCommonErrors.CustomValidation(ClassName, "InvalidAddingReason", "The reason provided is not valid for adding points.");
    public static readonly Error InvalidReason = DomainCommonErrors.InvalidProp(ClassName, "PointTransactionReason", "Point Transaction Reason", "Invalid reason value");

}
