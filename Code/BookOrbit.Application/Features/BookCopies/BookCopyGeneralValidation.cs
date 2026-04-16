namespace BookOrbit.Application.Features.BookCopies;
public static class BookCopyGeneralValidation
{
    static public IRuleBuilder<T, Guid> BookCopyIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
    ruleBuilder
        .NotEmpty().WithMessage(BookCopyErrors.IdRequired.Description)
        .Must(id => id != Guid.Empty).WithMessage(BookCopyErrors.IdRequired.Description);


    static public IRuleBuilder<T, BookCopyCondition> BookCopyConditionRules<T>(this IRuleBuilder<T, BookCopyCondition> ruleBuilder) =>
         ruleBuilder.
         IsInEnum().WithMessage(BookCopyErrors.InvalidCondition.Description);

}