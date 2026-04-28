namespace BookOrbit.Application.Features.BorrowingReviews.Commands.CreateBorrowingReview;

public class CreateBorrowingReviewCommandValidator : AbstractValidator<CreateBorrowingReviewCommand>
{
    public CreateBorrowingReviewCommandValidator()
    {
        RuleFor(x => x.ReviewerStudentId)
            .Cascade(CascadeMode.Stop)
            .ReviewerStudentIdRules();

        RuleFor(x => x.ReviewedStudentId)
            .Cascade(CascadeMode.Stop)
            .ReviewedStudentIdRules();

        RuleFor(x => x.BorrowingTransactionId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionIdRules();

        RuleFor(x => x.Rating)
            .Cascade(CascadeMode.Stop)
            .BorrowingReviewRatingRules();
    }
}
