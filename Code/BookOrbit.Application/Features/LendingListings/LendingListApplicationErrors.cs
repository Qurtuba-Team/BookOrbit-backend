namespace BookOrbit.Application.Features.LendingListings;
static public class LendingListApplicationErrors
{
    public static readonly Error BookCopyAlreadyListed = ApplicationCommonErrors.CustomConflict(
        LendingListRecordErrors.ClassName,
        "BookCopyAlreadyListed",
        "The book copy is already listed for lending.");
    public static readonly Error BookCopyIsNotAvilableForlending = ApplicationCommonErrors.CustomConflict(
        LendingListRecordErrors.ClassName,
        "BookCopyIsNotAvilableForlending",
        "The book copy is not available for lending.");

        public static readonly Error StateIsNotReserved = ApplicationCommonErrors.CustomConflict(
            LendingListRecordErrors.ClassName,
            "StateIsNotReserved",
            "The lending list record is not in reserved state.");

    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundProp(
    LendingListRecordErrors.ClassName,
    "LendingListRecord",
    "Lending List Record");
}