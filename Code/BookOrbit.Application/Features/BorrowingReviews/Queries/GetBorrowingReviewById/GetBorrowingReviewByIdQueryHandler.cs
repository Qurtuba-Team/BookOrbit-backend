
namespace BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviewById;

public class GetBorrowingReviewByIdQueryHandler(
    ILogger<GetBorrowingReviewByIdQueryHandler> logger,
    IAppDbContext context)
    : IRequestHandler<GetBorrowingReviewByIdQuery, Result<BorrowingReviewDto>>
{
    public async Task<Result<BorrowingReviewDto>> Handle(GetBorrowingReviewByIdQuery query, CancellationToken ct)
    {
        var review = await context.BorrowingReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(br => br.Id == query.BorrowingReviewId, ct);

        if (review is null)
        {
            logger.LogWarning("Borrowing review {BorrowingReviewId} not found.", query.BorrowingReviewId);
            return BorrowingReviewApplicationErrors.NotFoundById;
        }

        return BorrowingReviewDto.FromEntity(review);
    }
}
