using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;

namespace BookOrbit.Application.Features.BorrowingReviews;

public static class BorrowingReviewGeneralValidation
{
    public static IRuleBuilder<T, Guid> BorrowingReviewIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingReviewErrors.IdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingReviewErrors.IdRequired.Description);

    public static IRuleBuilder<T, Guid> ReviewerStudentIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingReviewErrors.ReviewerStudentIdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingReviewErrors.ReviewerStudentIdRequired.Description);

    public static IRuleBuilder<T, Guid> ReviewedStudentIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingReviewErrors.ReviewedStudentIdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingReviewErrors.ReviewedStudentIdRequired.Description);

    public static IRuleBuilder<T, Guid> BorrowingTransactionIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(BorrowingReviewErrors.BorrowingTransactionIdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(BorrowingReviewErrors.BorrowingTransactionIdRequired.Description);

    public static IRuleBuilder<T, int> BorrowingReviewRatingRules<T>(this IRuleBuilder<T, int> ruleBuilder) =>
        ruleBuilder
            .InclusiveBetween(StartsRating.MinRating, StartsRating.MaxRating)
            .WithMessage(BorrowingReviewErrors.InvalidRating.Description);
}
