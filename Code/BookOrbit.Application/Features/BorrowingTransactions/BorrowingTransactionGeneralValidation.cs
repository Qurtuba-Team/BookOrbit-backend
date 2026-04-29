namespace BookOrbit.Application.Features.BorrowingTransactions;
public static class BorrowingTransactionGeneralValidation
{
    public static IRuleBuilder<T, Guid> BorrowingTransactionIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingTransactionErrors.IdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingTransactionErrors.IdRequired.Description);

    public static IRuleBuilder<T, Guid> BorrowingRequestIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingTransactionErrors.BorrowingRequestIdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingTransactionErrors.BorrowingRequestIdRequired.Description);
}
