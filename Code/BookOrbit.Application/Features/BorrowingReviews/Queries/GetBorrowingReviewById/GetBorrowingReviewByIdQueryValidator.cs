
namespace BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviewById;

public class GetBorrowingReviewByIdQueryValidator : AbstractValidator<GetBorrowingReviewByIdQuery>
{
    public GetBorrowingReviewByIdQueryValidator()
    {
        RuleFor(x => x.BorrowingReviewId)
            .Cascade(CascadeMode.Stop)
            .BorrowingReviewIdRules();
    }
}
