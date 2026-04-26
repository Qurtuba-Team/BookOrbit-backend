namespace BookOrbit.Application.Features.LendingListings;
static public class LendingListApplicationErrors
{
    private const string ClassName = nameof(LendingListRecord);
    public static readonly Error BookCopyAlreadyListed = ApplicationCommonErrors.CustomConflict(
        ClassName,
        "BookCopyAlreadyListed",
        "The book copy is already listed for lending.");
    public static readonly Error BookCopyIsNotAvilableForlending = ApplicationCommonErrors.CustomConflict(
        ClassName,
        "BookCopyIsNotAvilableForlending",
        "The book copy is not available for lending.");

        public static readonly Error StateIsNotReserved = ApplicationCommonErrors.CustomConflict(
            ClassName,
            "StateIsNotReserved",
            "The lending list record is not in reserved state.");

    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundProp(
    ClassName,
    "LendingListRecord",
    "Lending List Record");
}