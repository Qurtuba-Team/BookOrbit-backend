
namespace BookOrbit.Application.Features.PointTransactions;

static public class PointTransactionGeneralValidation
{
    static public IRuleBuilder<T, Guid> PointTransactionIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(PointTransactionErrors.IdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(PointTransactionErrors.IdRequired.Description);
}
