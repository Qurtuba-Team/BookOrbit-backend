namespace BookOrbit.Domain.LendingListings;

static public class LendingListRecordErrors
{
    private const string ClassName = nameof(LendingListRecord);

    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    static public readonly Error CostRequeired = DomainCommonErrors.RequiredProp(ClassName, "Cost", "Cost");
    static public readonly Error BookCopyIdRequired = DomainCommonErrors.RequiredProp(ClassName, "BookCopyId", "Book Copy Id");
    static public readonly Error InvalidBorrowingDuration = DomainCommonErrors.InvalidProp(ClassName, "BorrowingDurationInDays", "Borrowing Duration In Days", $"It must be between {LendingListRecord.MinBorrowingDurationInDays} and {LendingListRecord.MaxBorrowingDurationInDays} days.");
    static public readonly Error InvalidCostInPoints = DomainCommonErrors.InvalidProp(ClassName, "CostInPoints", "Cost In Points", $"It must be between {LendingListRecord.MinCostInPoints} and {LendingListRecord.MaxCostInPoints} points.");
    static public readonly Error InvalidExpirationDate = DomainCommonErrors.DateShouldBeInFuture(ClassName, "ExpirationDate", "Expiration Date");

    static public Error InvalidStateTransition(LendingListRecordState currentState, LendingListRecordState newState)
    {
        return DomainCommonErrors.InvalidStateTransition(ClassName, currentState.ToString(), newState.ToString());
    }
}
