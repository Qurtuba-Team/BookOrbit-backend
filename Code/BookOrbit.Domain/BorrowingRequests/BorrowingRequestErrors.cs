using BookOrbit.Domain.BorrowingRequests.Enums;

namespace BookOrbit.Domain.BorrowingRequests;

static public class BorrowingRequestErrors
{
    private const string ClassName = nameof(BorrowingRequest);

    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    static public readonly Error BorrowingStudentIdRequired = DomainCommonErrors.RequiredProp(ClassName, "BorrowingStudentId", "Borrowing Student Id");
    static public readonly Error LendingRecordIdRequired = DomainCommonErrors.RequiredProp(ClassName, "LendingRecordId", "Lending Record Id");
    static public readonly Error InvalidExpirationDate = DomainCommonErrors.DateShouldBeInFuture(ClassName, "ExpirationDate", "Expiration Date");

    public static Error InvalidStateTransition(BorrowingRequestState currentState, BorrowingRequestState newState)
    {
        if (currentState == newState)
            return DomainCommonErrors.InvalidStateTransitionSameState(ClassName, currentState.ToString());

        return DomainCommonErrors.InvalidStateTransition(ClassName, currentState.ToString(), newState.ToString());
    }
}
