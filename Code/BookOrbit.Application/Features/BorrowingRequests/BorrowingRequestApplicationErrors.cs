namespace BookOrbit.Application.Features.BorrowingRequests;
static public class BorrowingRequestApplicationErrors
{
    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundProp(
        BorrowingRequestErrors.ClassName,
        "BorrowingRequest",
        "Borrowing Request");

    public static readonly Error LendingRecordNotAvailable = ApplicationCommonErrors.CustomConflict(
        BorrowingRequestErrors.ClassName,
        "LendingRecordNotAvailable",
        "The lending record is not available for borrowing requests.");

    public static readonly Error AlreadyExists = ApplicationCommonErrors.CustomConflict(
        BorrowingRequestErrors.ClassName,
        "AlreadyExists",
        "A borrowing request already exists for the specified student and lending record.");

    public static readonly Error StudentCannotBorrowOwnedCopies = ApplicationCommonErrors.CustomConflict(
        BorrowingRequestErrors.ClassName,
        "StudentCannotBorrowOwnedCopies",
        "A student cannot request to borrow their own book copies.");

    public static readonly Error StudentNotLendingRecordOwner = ApplicationCommonErrors.CustomUnauthorized(
        BorrowingRequestErrors.ClassName,
        "StudentNotLendingRecordOwner",
        "Student is not the owner of the lending record.");

    public static readonly Error StudentNotBorrower = ApplicationCommonErrors.CustomUnauthorized(
        BorrowingRequestErrors.ClassName,
        "StudentNotBorrower",
        "Student is not the borrower for this request.");

    public static readonly Error BorrowingRequestNotAccepted = ApplicationCommonErrors.CustomConflict(
        BorrowingRequestErrors.ClassName,
        "BorrowingRequestNotAccepted",
        "The borrowing request is not in accepted state.");

   public static readonly Error NotOwnerOfLendingRecord = ApplicationCommonErrors.CustomUnauthorized(
        BorrowingRequestErrors.ClassName,
        "NotOwnerOfLendingRecord",
        "The student is not the owner of the lending record associated with this borrowing request.");

    public static readonly Error NotEnoughPoints = ApplicationCommonErrors.CustomConflict(
        BorrowingRequestErrors.ClassName,
        "NotEnoughPoints",
        "The student does not have enough points to request this lending record.");
}
