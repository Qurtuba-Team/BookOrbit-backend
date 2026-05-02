
namespace BookOrbit.Application.Features.BorrowingReviews.Commands.CreateBorrowingReview;

public class CreateBorrowingReviewCommandHandler(
    ILogger<CreateBorrowingReviewCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<CreateBorrowingReviewCommand, Result<BorrowingReviewDto>>
{
    public async Task<Result<BorrowingReviewDto>> Handle(CreateBorrowingReviewCommand command, CancellationToken ct)
    {
        var transactionData = await context.BorrowingTransactions
            .AsNoTracking()
            .Select(bt => new
            {
                bt.Id,
                bt.State,
                ReviewerStudentId = bt.LenderStudentId,
                ReviewedStudentId = bt.BorrowerStudentId
            })
            .FirstOrDefaultAsync(bt => bt.Id == command.BorrowingTransactionId, ct);

        if (transactionData is null)
        {
            logger.LogWarning("Borrowing transaction not found. BorrowingTransactionId: {BorrowingTransactionId}", command.BorrowingTransactionId);
            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        if(transactionData.State is not
            BorrowingTransactionState.Returned
            or BorrowingTransactionState.Overdue)
        {
            logger.LogWarning("Borrowing transaction is not in a returned/overdue state. BorrowingTransactionId: {BorrowingTransactionId}", command.BorrowingTransactionId);
            return BorrowingTransactionApplicationErrors.InvalidState;
        }

        var reviewAlreadyExists = await context.BorrowingReviews
            .AsNoTracking()
            .AnyAsync(br => br.BorrowingTransactionId == command.BorrowingTransactionId, ct);

        if (reviewAlreadyExists)
        {
            logger.LogWarning("Borrowing review already exists for transaction. BorrowingTransactionId: {BorrowingTransactionId}", command.BorrowingTransactionId);
            return BorrowingReviewApplicationErrors.AlreadyExists;
        }

        var ratingResult = StarsRating.Create(command.Rating);

        if (ratingResult.IsFailure)
        {
            logger.LogWarning("Invalid rating for borrowing review. Errors: {Errors}", ratingResult.Errors);
            return ratingResult.Errors;
        }

        var reviewResult = BorrowingReview.Create(
            Guid.NewGuid(),
            transactionData.ReviewerStudentId,
            transactionData.ReviewedStudentId,
            command.BorrowingTransactionId,
            command.Description,
            ratingResult.Value);

        if (reviewResult.IsFailure)
        {
            logger.LogWarning("Failed to create borrowing review. Errors: {Errors}", reviewResult.Errors);
            return reviewResult.Errors;
        }

        context.BorrowingReviews.Add(reviewResult.Value);
        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingReviewCachingConstants.BorrowingReviewTag, ct);

        logger.LogInformation("Borrowing review {BorrowingReviewId} created.", reviewResult.Value.Id);

        return BorrowingReviewDto.FromEntity(reviewResult.Value);
    }
}
