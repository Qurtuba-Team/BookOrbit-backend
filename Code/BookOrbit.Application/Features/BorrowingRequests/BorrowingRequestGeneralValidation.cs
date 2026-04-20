namespace BookOrbit.Application.Features.BorrowingRequests;
static public class BorrowingRequestGeneralValidation
{
    public static IRuleBuilder<T, Guid> BorrowingRequestIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingRequestErrors.IdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingRequestErrors.IdRequired.Description);

    public static IRuleBuilder<T, Guid> BorrowingStudentIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingRequestErrors.BorrowingStudentIdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingRequestErrors.BorrowingStudentIdRequired.Description);

    public static IRuleBuilder<T, Guid> LendingRecordIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingRequestErrors.LendingRecordIdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingRequestErrors.LendingRecordIdRequired.Description);
}
