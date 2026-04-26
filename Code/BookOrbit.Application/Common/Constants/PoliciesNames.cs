namespace BookOrbit.Application.Common.Constants;
static public class PoliciesNames
{
    // Only Users Who Confirmed Their Emails
    public const string ActiveUserPolicy = "ActiveUserAccess";

    // Only User Who Is Saved In The System (Authenticated)
    public const string RegisteredUserPolicy = "RegisteredUserAccess";

    // Only Users Who Are Saved In The System (Authenticated)
    // And Has The Same Id Of Resource's User ID
    public const string RegisteredUserOwnershipPolicy = "RegisteredUserOwnershipAccess";

    // Only Users With Role ["Admin"]
    public const string AdminOnlyPolicy = "AdminOnlyAccess";

    // Only Users With Role ["Admin"] 
    // Or Users With Role ["Student"] But Has The Same Id Of The Resource's student Id
    // Doesn't Have To be confirmed
    public const string StudentOwnershipPolicy = "StudentOwnerShipAccess";

    // No role Except Student, Even Admin
    public const string StudentOnlyPolicy = "StudentOnlyAccess";

    // Student With Active State
    public const string ActiveStudentPolicy = "ActiveStudentAccess";

    // Student Who Is Borrowing Student In The Borrowing Request
    public const string BorrowingRequestBorrowingStudentPolicy = "BorrowingRequestBorrowingStudentAccess";

    // Student Who Is Lending Student In The Borrowing Request
    public const string BorrowingRequestLendingStudentPolicy = "BorrowingRequestLendingStudentAccess";

    // Student Who Is Borrowing Student In The Borrowing Request Or Lending Student
    public const string BorrowingRequestRelatedStudentPolicy = "BorrowingRequestRelatedStudentAccess";

    //Student who is owner of the book copy 
    public const string StudentOwnerOfBookCopyPolicy = "StudentOwnerOfBookCopyAccess";

    //Student who is owner of the lending list record
    public const string StudentOwnerOfLendingListRecordPolicy = "StudentOwnerOfLendingListRecordAccess";

    //Student who is borrowing student in the borrowing transaction record
    public const string BorrowingTransactionBorrowingStudentPolicy = "BorrowingTransactionBorrowingStudentAccess";

    //Student who is lending student in the borrowing transaction record
    public const string BorrowingTransactionLendingStudentPolicy = "BorrowingTransactionLendingStudentAccess";

    //Student who is related to the borrowing transaction record (borrowing student or lending student)
    public const string BorrowingTransactionRelatedStudentPolicy = "BorrowingTransactionRelatedStudentAccess";

    //Student who is accepted for the lending list record
    public const string StudentAcceptedForLendingListRecordPolicy = "StudentAcceptedForLendingListRecordAccess";
}
