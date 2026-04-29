
namespace BookOrbit.Application.Features.BorrowingTransactionEvents;

public static class BorrowingTransactionEventGeneralValidation
{
    public static IRuleBuilder<T, Guid> BorrowingTransactionEventIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingTransactionEventErrors.IdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingTransactionEventErrors.IdRequired.Description);

    public static IRuleBuilder<T, Guid> BorrowingTransactionIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingTransactionEventErrors.BorrowingTransactionIdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingTransactionEventErrors.BorrowingTransactionIdRequired.Description);
}
