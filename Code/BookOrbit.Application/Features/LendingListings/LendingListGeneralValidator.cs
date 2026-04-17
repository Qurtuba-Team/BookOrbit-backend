namespace BookOrbit.Application.Features.LendingListings;
static public class LendingListGeneralValidator
{
    static public IRuleBuilder<T, Guid> LendingListIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
    ruleBuilder
        .NotEmpty().WithMessage(LendingListRecordErrors.IdRequired.Description)
        .Must(id => id != Guid.Empty).WithMessage(LendingListRecordErrors.IdRequired.Description);

    static public IRuleBuilder<T, int> BorrowingDurationInDaysRules<T>(this IRuleBuilder<T, int> ruleBuilder) =>
            ruleBuilder
            .GreaterThanOrEqualTo(LendingListRecord.MinBorrowingDurationInDays).WithMessage(LendingListRecordErrors.InvalidBorrowingDuration.Description)
            .LessThanOrEqualTo(LendingListRecord.MaxBorrowingDurationInDays).WithMessage(LendingListRecordErrors.InvalidBorrowingDuration.Description);


}