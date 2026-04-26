namespace BookOrbit.Application.Features.BorrowingRequests;
static public class BorrowingRequestApplicationErrors
{
    private const string ClassName = nameof(BorrowingRequest);

    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundProp(
        ClassName,
        "BorrowingRequest",
        "Borrowing Request");

    public static readonly Error LendingRecordNotAvailable = ApplicationCommonErrors.CustomConflict(
        ClassName,
        "LendingRecordNotAvailable",
        "The lending record is not available for borrowing requests.");

    public static readonly Error AlreadyExists = ApplicationCommonErrors.CustomConflict(
        ClassName,
        "AlreadyExists",
        "A borrowing request already exists for the specified student and lending record.");

    public static readonly Error StudentCannotBorrowOwnedCopies = ApplicationCommonErrors.CustomConflict(
        ClassName,
        "StudentCannotBorrowOwnedCopies",
        "A student cannot request to borrow their own book copies.");

    public static readonly Error StudentNotLendingRecordOwner = ApplicationCommonErrors.CustomUnauthorized(
        ClassName,
        "StudentNotLendingRecordOwner",
        "Student is not the owner of the lending record.");

    public static readonly Error StudentNotBorrower = ApplicationCommonErrors.CustomUnauthorized(
        ClassName,
        "StudentNotBorrower",
        "Student is not the borrower for this request.");

    public static readonly Error BorrowingRequestNotAccepted = ApplicationCommonErrors.CustomConflict(
        ClassName,
        "BorrowingRequestNotAccepted",
        "The borrowing request is not in accepted state.");

   public static readonly Error NotOwnerOfLendingRecord = ApplicationCommonErrors.CustomUnauthorized(
        ClassName,
        "NotOwnerOfLendingRecord",
        "The student is not the owner of the lending record associated with this borrowing request.");

    public static readonly Error NotEnoughPoints = ApplicationCommonErrors.CustomConflict(
        ClassName,
        "NotEnoughPoints",
        "The student does not have enough points to request this lending record.");
}
